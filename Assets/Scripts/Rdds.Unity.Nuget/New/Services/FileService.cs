﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.New.Services
{
  public class FileService
  {
    public string TempDirectoryPath
    {
      get
      {
        var path = Path.Combine(Path.GetTempPath(), "NugetForUnity");
        
        if (!Directory.Exists(path)) 
          Directory.CreateDirectory(path);

        return path;
      }
    }
    
    public string Unzip(string nupkgPath, string targetDirectory)
    {
      using var zip = ZipFile.OpenRead(nupkgPath);
      var baseDirectory = Path.Combine(targetDirectory, Path.GetFileNameWithoutExtension(nupkgPath));
        
      foreach (var entry in zip.Entries)
      {
        var filePath = Path.Combine(baseDirectory, entry.FullName);
        var directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
        entry.ExtractToFile(filePath, true);
      }

      return baseDirectory;
    }

    public string? ReadFile(string filePath)
    {
      if (!File.Exists(filePath))
        return null;

      return File.ReadAllText(filePath);
    }

    public async Task<string?> ReadFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
      var bytes = await ReadBytesAsync(filePath, cancellationToken);

      if (bytes == null)
        return null;

      return Encoding.Default.GetString(bytes);
    }
    
    public async Task<string> RequireFileContentAsync(string filePath, CancellationToken cancellationToken)
    {
      var content = await ReadFromFileAsync(filePath, cancellationToken);

      if (content == null)
        throw new FileNotFoundException($"File {filePath} not found");

      return content;
    }
    
    public async Task<byte[]?> ReadBytesAsync(string filePath, CancellationToken cancellationToken)
    {
      if (!File.Exists(filePath))
        return null;
      
      using var stream = File.OpenRead(filePath);
      var bytes = new byte[stream.Length];
      await stream.ReadAsync(bytes, 0, (int)stream.Length, cancellationToken);
      return bytes;
    }
    
    public async Task WriteToFileAsync(string filePath, string content)
    {
      var bytes = Encoding.Default.GetBytes(content);
      using var stream = CreateWriteFileStream(filePath);
      stream.Seek(0, SeekOrigin.End);
      await stream.WriteAsync(bytes, 0, bytes.Length);
    }
    
    public FileStream CreateWriteFileStream(string filePath) => File.Open(filePath, FileMode.Create, FileAccess.Write);

    public bool RemoveFile(string filePath)
    {
      if (!File.Exists(filePath))
        return false;

      try
      {
        File.Delete(filePath);
        return true;
      }
      catch (IOException ex)
      {
        LogHelper.LogWarningException($"Failed remove file {filePath}", ex);
        return false;
      }
    }

    public string GetNupkgTempFilePath(PackageIdentity identity) => 
      Path.Combine(TempDirectoryPath, $"{identity.Id}.{identity.Version.OriginalString}.nupkg");

    public IEnumerable<string> FindFiles(string directory, string extension, bool recursive) =>
      Directory.EnumerateFiles(directory, $"*.{extension}",
        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

    public IEnumerable<string> CopyFilesFromDirectory(string directory, string extension, string destinationDirectory)
    {
      var files = FindFiles(directory, extension, false);
      var copiedFiles = new List<string>();
      
      foreach (var file in files)
      {
        var newFilePath = CopyFile(file, destinationDirectory);

        if (newFilePath != null) 
          copiedFiles.Add(file);
      }

      return copiedFiles;
    }

    public string? CopyFile(string sourceFile, string destinationDirectory)
    {
      var newFilePath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
      
      try
      {
        File.Copy(sourceFile, newFilePath);
      }
      catch (IOException ex)
      {
        LogHelper.LogWarningException($"Couldn't copy file {sourceFile} to destination {destinationDirectory}", ex);
        return null;
      }
      
      return newFilePath;
    }
  }
}
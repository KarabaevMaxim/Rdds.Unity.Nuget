using System.IO;
using System.IO.Compression;

namespace Rdds.Unity.Nuget.Services
{
  public class FileService
  {
    public void Unzip(string nupkgPath, string targetDirectory)
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
    }

    public string? ReadFile(string filePath)
    {
      if (!File.Exists(filePath))
        return null;

      return File.ReadAllText(filePath);
    }

    public FileStream CreateWriteFileStream(string filePath) => File.OpenWrite(filePath);
  }
}
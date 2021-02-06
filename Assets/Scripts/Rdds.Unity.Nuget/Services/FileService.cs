using System.IO;
using System.IO.Compression;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;

namespace Rdds.Unity.Nuget.Services
{
  public class FileService
  {
    // public void Unzip(string nupkgPath)
    // {
    //   using (ZipArchive zip = ZipFile.OpenRead(nupkgPath))
    //   {
    //     string baseDirectory = Path.Combine(NugetConfigFile.RepositoryPath, Path.GetFileNameWithoutExtension(nupkgPath));
    //     
    //     foreach (ZipArchiveEntry entry in zip.Entries)
    //     {
    //       string filePath = Path.Combine(baseDirectory, entry.FullName);
    //       string directory = Path.GetDirectoryName(filePath);
    //       Directory.CreateDirectory(directory);
    //
    //       entry.ExtractToFile(filePath, overwrite: true);
    //     }
    //   }
    // }

    public string? ReadFile(string filePath)
    {
      if (!File.Exists(filePath))
        return null;

      return File.ReadAllText(filePath);
    }

    public FileStream CreateWriteFileStream(string filePath) => File.OpenWrite(filePath);
  }
}
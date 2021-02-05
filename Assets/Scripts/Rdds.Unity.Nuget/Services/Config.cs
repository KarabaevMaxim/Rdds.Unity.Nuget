using System.IO;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.Services
{
  public static class Config
  {
    public static string CacheDirectoryPath { get; } = "D:/Cache";

    public static string GetCacheNupkgFileName(PackageIdentity identity)
    {
      return Path.Combine(CacheDirectoryPath, $"{identity.Id}.{identity.Version.OriginalString}.nupkg");
    }
  }
}
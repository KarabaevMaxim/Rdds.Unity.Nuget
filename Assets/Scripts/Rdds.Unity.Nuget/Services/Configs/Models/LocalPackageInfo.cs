using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Services.Configs.Models
{
  internal readonly struct LocalPackageInfo
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public string Path { get; }

    public LocalPackageInfo(string id, string version, string path)
    {
      Id = id;
      Version = version;
      Path = path;
    }
    
    internal class IdVersionComparer : IEqualityComparer<LocalPackageInfo>
    {
      public bool Equals(LocalPackageInfo x, LocalPackageInfo y) => 
        x.Id == y.Id && x.Version == y.Version;

      public int GetHashCode(LocalPackageInfo obj)
      {
        unchecked
        {
          var hashCode = obj.Id.GetHashCode();
          hashCode = (hashCode * 397) ^ obj.Version.GetHashCode();
          return hashCode;
        }
      }

      public static IdVersionComparer New => new IdVersionComparer();
    }
  }
}
using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Services.New.Configs.Models
{
  public class InstalledPackageInfo
  {
    public string Id { get; }
      
    public string Version { get; }
      
    public List<string> InstalledInAssemblies { get; }
      
    public IEnumerable<string> DllNames { get; }

    public InstalledPackageInfo(string id, string version, List<string> installedInAssemblies, IEnumerable<string> dllNames)
    {
      Id = id;
      Version = version;
      InstalledInAssemblies = installedInAssemblies;
      DllNames = dllNames;
    }
  }
}
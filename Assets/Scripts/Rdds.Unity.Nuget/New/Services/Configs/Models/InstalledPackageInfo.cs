using System.Collections.Generic;

namespace Rdds.Unity.Nuget.New.Services.Configs.Models
{
  internal class InstalledPackageInfo
  {
    public string Id { get; }
      
    public string Version { get; }
      
    public List<string> InstalledInAssemblies { get; }
      
    public List<string> DllNames { get; }

    public InstalledPackageInfo(string id, string version, List<string> installedInAssemblies, List<string> dllNames)
    {
      Id = id;
      Version = version;
      InstalledInAssemblies = installedInAssemblies;
      DllNames = dllNames;
    }
  }
}
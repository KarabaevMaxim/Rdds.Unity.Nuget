using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities.Config
{
  public class NugetConfigFile
  {
    public IEnumerable<NugetPackageSource> PackageSources { get; set; } = null!;

    public string RepositoryPath { get; set; } = null!;
  }
}
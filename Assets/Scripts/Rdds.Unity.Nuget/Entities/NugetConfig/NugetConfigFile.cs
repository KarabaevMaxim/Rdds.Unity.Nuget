using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities.NugetConfig
{
  public class NugetConfigFile
  {
    public IEnumerable<NugetPackageSource> PackageSources { get; }

    public string RepositoryPath { get; }

    public NugetConfigFile(IEnumerable<NugetPackageSource> packageSources, string repositoryPath)
    {
      PackageSources = packageSources;
      RepositoryPath = repositoryPath;
    }
  }
}
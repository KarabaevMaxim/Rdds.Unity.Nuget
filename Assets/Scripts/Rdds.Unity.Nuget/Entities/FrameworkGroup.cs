using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  public class FrameworkGroup
  {
    public Framework TargetFramework { get; }
    
    public IEnumerable<PackageIdentity> Dependencies { get; }

    public FrameworkGroup(Framework targetFramework, IEnumerable<PackageIdentity> dependencies)
    {
      TargetFramework = targetFramework;
      Dependencies = dependencies;
    }
  }
}
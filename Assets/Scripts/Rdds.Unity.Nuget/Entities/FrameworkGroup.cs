using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  public class FrameworkGroup
  {
    public string TargetFramework { get; set; }
    
    public IEnumerable<PackageIdentity> Dependencies { get; set; }
  }
}
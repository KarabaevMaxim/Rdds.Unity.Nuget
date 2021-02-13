using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  internal class AssemblyModel
  {
    public string Name { get; set; } = null!;

    public string RootNamespace { get; set; } = null!;
    
    public bool OverrideReferences { get; set; }
    
    public List<string>? PrecompiledReferences { get; set; }
  }
}
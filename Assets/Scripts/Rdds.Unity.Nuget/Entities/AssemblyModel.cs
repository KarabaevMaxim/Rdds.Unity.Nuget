using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rdds.Unity.Nuget.Entities
{
  internal class AssemblyModel
  {
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("rootNameSpace")]
    public string RootNamespace { get; set; } = null!;
    
    [JsonProperty("overrideReferences")]
    public bool OverrideReferences { get; set; }
    
    [JsonProperty("precompiledReferences")]
    public List<string>? PrecompiledReferences { get; set; }

    [JsonIgnore] 
    public string Path { get; set; } = null!;

    public override bool Equals(object obj) => 
      ReferenceEquals(this, obj) || (obj is AssemblyModel other && Equals(other));

    private bool Equals(AssemblyModel other) => Name == other.Name;
  }
}
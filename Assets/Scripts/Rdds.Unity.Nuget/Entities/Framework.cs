using System;
using NuGet.Frameworks;

namespace Rdds.Unity.Nuget.Entities
{
  public class Framework
  {
    // ReSharper disable once InconsistentNaming
    public string TFM { get; }

    public NuGetFramework ToNugetFramework() => NuGetFramework.Parse(TFM);

    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj) || obj is Framework other && TFM.Equals(other.TFM, StringComparison.InvariantCultureIgnoreCase);

    public override int GetHashCode() => TFM.GetHashCode();

    // Taken from https://github.com/GlitchEnzo/NuGetForUnity
    private string NormalizeTFM(string targetFramework)
    {
      var convertedTargetFramework = targetFramework
        .ToLower()
        .Replace(".netstandard", "netstandard")
        .Replace("native0.0", "native");

      convertedTargetFramework = convertedTargetFramework.StartsWith(".netframework") ?
        convertedTargetFramework.Replace(".netframework", "net").Replace(".", string.Empty) :
        convertedTargetFramework;

      return convertedTargetFramework;
    }
    
    public Framework(string name) => TFM = NormalizeTFM(name);
  }
}
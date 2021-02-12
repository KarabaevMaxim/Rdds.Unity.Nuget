using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Rdds.Unity.Nuget.Entities
{
  public sealed class PackageVersion
  {
    public int Major { get; }
    
    public int Minor { get; }
    
    public int Patch { get; }

    public IEnumerable<string> Labels { get; }
    
    public string OriginalString { get; }

    public override string ToString()
    {
      var labels = Labels.Any() ? $"-{string.Join(".", Labels)}" : string.Empty; 
      return $"{Major}.{Minor}.{Patch}{labels}";
    }

    public override bool Equals(object obj)
    {
      if (obj is PackageVersion other)
        return ToString() == other.ToString();

      return false;
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = OriginalString.GetHashCode();
        hashCode = (hashCode * 397) ^ Major;
        hashCode = (hashCode * 397) ^ Minor;
        hashCode = (hashCode * 397) ^ Patch;
        hashCode = (hashCode * 397) ^ Labels.GetHashCode();
        return hashCode;
      }
    }

    /// <summary>
    /// {major}.{minor}.{patch}[-{tag1}.{tag2}.{tagN}]
    /// </summary>
    public static PackageVersion Parse(string source)
    {
      var parts = source.Split(new[] {"-"}, 2, StringSplitOptions.RemoveEmptyEntries);
      var semantic = parts[0];
      var values = semantic.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);
      var major = int.Parse(values[0]);
      var minor = int.Parse(values[1]);
      var patch = int.Parse(values[2]);
      var tags = parts.Length > 1 ? parts[1].Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries) : new string[0];
      return new PackageVersion(major, minor, patch, tags);
    }

    public static bool TryParse(string source, out PackageVersion? result)
    {
      try
      {
        result = Parse(source);
        return true;
      }
      catch
      {
        result = null;
        return false;
      }
    }

    public PackageVersion(int major, int minor, int patch, IEnumerable<string> labels, string? originalString = null)
    {
      Major = major;
      Minor = minor;
      Patch = patch;
      Labels = labels;
      OriginalString = originalString ?? ToString();
    }
  }
}
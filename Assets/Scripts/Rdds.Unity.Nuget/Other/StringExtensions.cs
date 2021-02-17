using System;

namespace Rdds.Unity.Nuget.Other
{
  internal static class StringExtensions
  {
    public static bool ContainsIgnoreCase(this string source, string finding) => 
      source.IndexOf(finding, StringComparison.OrdinalIgnoreCase) != -1;
  }
}
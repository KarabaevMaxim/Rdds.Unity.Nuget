using System;
using System.Text;

namespace Rdds.Unity.Nuget.Other
{
  internal static class StringExtensions
  {
    public static bool ContainsIgnoreCase(this string source, string finding) => 
      source.IndexOf(finding, StringComparison.OrdinalIgnoreCase) != -1;
    
    public static string RemoveUTF8Preamble(this string source)
    {
      var preamble = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

      return source.StartsWith(preamble) 
        ? source.Remove(0, preamble.Length) 
        : source;
    }
  }
}
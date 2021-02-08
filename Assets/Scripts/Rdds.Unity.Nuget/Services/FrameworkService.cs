using System.Collections.Generic;
using UnityEditor;

namespace Rdds.Unity.Nuget.Services
{
  public class FrameworkService
  {
    private static readonly string[] _unityFrameworks = { "unity" };
    private static readonly string[] _netStandardFrameworks = {
      "netstandard2.0", "netstandard1.6", "netstandard1.5", "netstandard1.4", "netstandard1.3", "netstandard1.2", "netstandard1.1", "netstandard1.0" };
    private static readonly string[] _net4Unity2018Frameworks = { "net471", "net47" };
    private static readonly string[] _net4Unity2017Frameworks = { "net462", "net461", "net46", "net452", "net451", "net45", "net403", "net40", "net4" };
    private static readonly string[] _net3Frameworks = { "net35-unity full v3.5", "net35-unity subset v3.5", "net35", "net20", "net11" };
    private static readonly string[] _defaultFrameworks = { string.Empty };
    
    private readonly IReadOnlyDictionary<ApiCompatibilityLevel, string> _frameworks = new Dictionary<ApiCompatibilityLevel, string>
    {
      {ApiCompatibilityLevel.NET_2_0, "net20"},
      {ApiCompatibilityLevel.NET_4_6, "net46"},
      {ApiCompatibilityLevel.NET_2_0_Subset, ""},
      {ApiCompatibilityLevel.NET_Micro, ""},
      {ApiCompatibilityLevel.NET_Standard_2_0, "netstandard2.0"},
      {ApiCompatibilityLevel.NET_Web, ""},
    };
    
    public string GetFramework()
    {
     var apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup);
     return _frameworks[apiCompatibilityLevel];
    }
  }
}
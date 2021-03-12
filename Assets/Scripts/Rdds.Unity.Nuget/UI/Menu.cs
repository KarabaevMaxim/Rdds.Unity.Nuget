using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI
{
  internal static class Menu
  {
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowNugetWindow()
    {
      var wnd = EditorWindow.GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }
    
    [MenuItem("Rdds/Reset cache")]
    public static void ResetCache() => EditorPrefs.DeleteKey("MainWindowState");
  }
}
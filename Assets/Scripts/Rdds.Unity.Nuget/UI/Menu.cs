using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI
{
  internal static class Menu
  {
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowNugetWindow()
    {
      // todo todo move to StateService
      EditorPrefs.DeleteKey("MainWindowState");
      var wnd = EditorWindow.GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }
  }
}
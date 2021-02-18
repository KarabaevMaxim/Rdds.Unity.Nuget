using Rdds.Unity.Nuget.New.Presenter;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.UI
{
  internal static class Menu
  {
    [MenuItem("Rdds/New Unity.Nuget")]
    public static async void ShowNugetWindow()
    {
      EditorContext.NugetConfigService.LoadConfigFile();
      await EditorContext.PackagesFileService.LoadPackagesFileAsync();
      
      var wnd = EditorWindow.GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
      _ = new MainWindowPresenter(wnd);
    }
  }
}
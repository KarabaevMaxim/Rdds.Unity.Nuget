using JetBrains.Annotations;
using Rdds.Unity.Nuget.Presenter;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI
{
  internal static class Menu
  {
    [UsedImplicitly] 
    private static MainWindowPresenter _mainWindowPresenter = null!;
    
    [MenuItem("Rdds/New Unity.Nuget")]
    public static async void ShowNugetWindow()
    {
      var wnd = EditorWindow.GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
      _mainWindowPresenter = new MainWindowPresenter(wnd, 
        EditorContext.LocalPackagesService,
        EditorContext.InstalledPackagesConfigService,
        EditorContext.LocalPackagesConfigService, 
        EditorContext.NugetConfigService, 
        EditorContext.AssembliesService,
        EditorContext.RemotePackagesService,
        EditorContext.FrameworkService);
      await _mainWindowPresenter.InitializeAsync();
    }
  }
}
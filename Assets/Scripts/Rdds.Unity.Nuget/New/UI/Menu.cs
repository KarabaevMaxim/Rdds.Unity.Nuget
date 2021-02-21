using JetBrains.Annotations;
using Rdds.Unity.Nuget.New.Presenter;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.UI
{
  internal static class Menu
  {
    [UsedImplicitly] 
    private static MainWindowPresenter _mainWindowPresenter = null!;
    
    [MenuItem("Rdds/New Unity.Nuget")]
    public static async void ShowNugetWindow()
    {
      EditorContext.NugetConfigService.LoadConfigFile();
      await EditorContext.PackagesFileService.LoadPackagesFileAsync();
      
      var wnd = EditorWindow.GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
      _mainWindowPresenter = new MainWindowPresenter(wnd, EditorContext.LocalPackagesService, EditorContext.PackagesFileService,
        EditorContext.InstalledPackagesConfigService, EditorContext.LocalPackagesConfigService);
      await _mainWindowPresenter.InitializeAsync();
    }
  }
}
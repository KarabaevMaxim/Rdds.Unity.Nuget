using System;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.UI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  [Obsolete]
  public class MainWindow : EditorWindow
  {
    #region Unity methods
    
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }
    
    private async void OnEnable()
    {
      await EditorContext.NugetConfigService.LoadConfigFileAsync();
      await EditorContext.PackagesFileService.LoadConfigFileAsync();
      
      InitializeLayout();
    }

    #endregion
    
    private void InitializeLayout()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.MainWindowLayout);
      visualTree.CloneTree(rootVisualElement);
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.Styles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.CommonStyles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.TabStyles));
      
      InitializeTabControl();
    }

    private void InitializeTabControl()
    {
      var installedLayout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.InstalledTabLayout).CloneTree();
      var searchLayout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.SearchTabLayout).CloneTree();

      var installedTab = new InstalledPackagesTabControl(installedLayout.Q<VisualElement>("Root"), "Installed",
        null,
        EditorContext.InstalledPackagesService);

      var searchTab = new SearchPackagesTabControl(searchLayout.Q<VisualElement>("Root"), "Search",
        null,
        EditorContext.NugetConfigService, EditorContext.InstalledPackagesService, EditorContext.Logger);

      _ = new TabsControl(rootVisualElement.Q<VisualElement>("TabsControl"), installedTab, searchTab);
    }
  }
}
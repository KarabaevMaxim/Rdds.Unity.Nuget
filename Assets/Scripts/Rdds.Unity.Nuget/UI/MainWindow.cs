using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.UI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  public class MainWindow : EditorWindow
  {
    private readonly EditorContext _editorContext = EditorContext.Instance;
    
    #region Unity methods
    
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }
    
    private async void OnEnable()
    {
      EditorContext.Instance.NugetConfigService.LoadConfigFile();
      await EditorContext.Instance.PackagesFileService.LoadPackagesFileAsync();
      
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
        _editorContext.NugetService,
        _editorContext.InstalledPackagesService, _editorContext.Logger);

      var searchTab = new SearchPackagesTabControl(searchLayout.Q<VisualElement>("Root"), "Search",
        _editorContext.NugetService,
        _editorContext.NugetConfigService, _editorContext.InstalledPackagesService, _editorContext.Logger);

      _ = new TabsControl(rootVisualElement.Q<VisualElement>("TabsControl"), installedTab, searchTab);
    }
  }
}
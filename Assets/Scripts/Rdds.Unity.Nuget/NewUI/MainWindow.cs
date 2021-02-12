using System.Collections.Generic;
using Rdds.Unity.Nuget.NewUI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.NewUI
{
  public class MainWindow : EditorWindow
  {
    private VisualElement _leftPanel = null!;
    
    [MenuItem("Rdds/New Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }

    private void OnEnable()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.MainWindowLayout);
      visualTree.CloneTree(rootVisualElement);
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.Styles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UI.Paths.CommonStyles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.PackageRowStyles));

      _leftPanel = rootVisualElement.Q<VisualElement>("LeftPanel");

      var itemsSource = new List<PackageRowPresentationModel>
      {
        new PackageRowPresentationModel
        {
          Texture = null,
          Id = "rdds.client",
          Version = "0.0.1",
          Sources = new List<string> { "gitlab", "nuget.org", "other" }
        },
        new PackageRowPresentationModel
        {
          Texture = null,
          Id = "rdds.dto",
          Version = "0.0.1",
          Sources = new List<string> { "gitlab", "nuget.org", "other" }
        },
        new PackageRowPresentationModel
        {
          Texture = null,
          Id = "rdds.client",
          Version = "0.0.1",
          Sources = new List<string> { "gitlab", "nuget.org", "other" }
        },
        new PackageRowPresentationModel
        {
          Texture = null,
          Id = "rdds.client",
          Version = "0.0.1",
          Sources = new List<string> { "gitlab", "nuget.org", "other" }
        }
      };
      
      _ = new PackagesListControl(_leftPanel, "Installed", 200, itemsSource);
      _ = new PackagesListControl(_leftPanel, "Available", 200, itemsSource);
    }
  }
}
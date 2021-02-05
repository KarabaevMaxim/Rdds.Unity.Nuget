using System;
using System.Linq;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  public class MainWindow : EditorWindow
  {
    private readonly NugetService _nugetService = new NugetService();
    private VisualElement _container;
    private TextField _filterStringTextField;
    private Button _searchButton;
    
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }

    private void OnEnable()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/MainWindow.uxml");
      visualTree.CloneTree(rootVisualElement);
      var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/Styles.uss");
      rootVisualElement.styleSheets.Add(styleSheet);

      _container = rootVisualElement.Q<VisualElement>("Container");
      _filterStringTextField = rootVisualElement.Q<TextField>("FilterStringTextField");
      _searchButton = rootVisualElement.Q<Button>("SearchButton");

      _searchButton.clickable.clicked += async () =>
      {
        _container.Clear();
        var packages = await _nugetService.GetPackages(_filterStringTextField.text, 0, 20);

        foreach (var package in packages)
        {
          var control = new PackageControl(_container, package);
          await control.SetFields();
        }
      };
    }
  }
}
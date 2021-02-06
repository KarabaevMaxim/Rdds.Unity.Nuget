using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  public class MainWindow : EditorWindow
  {
    private NugetService _nugetService = null!;
    private FileService _fileService = null!;
    private NugetConfigService _nugetConfigService = null!;
    
    private VisualElement _container;
    private TextField _filterStringTextField;
    private Button _searchButton;
    private VisualElement _header;
    private PopupField<string> _sourcesControl;
    
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }

    private void OnEnable()
    {
      InitializeDependencies();
      InitializeLayout();
      
      _nugetConfigService.LoadConfigFile();

      CreateSourcesControl(_nugetConfigService.GetAvailableSources().ToList(), _nugetService.SelectedSource.Key);
    }
    
    private void InitializeDependencies()
    {
      _fileService = new FileService();
      _nugetConfigService = new NugetConfigService(_fileService);
      _nugetService = new NugetService(new UnityConsoleLogger(), _nugetConfigService, _fileService);
    }

    private void InitializeLayout()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/MainWindow.uxml");
      visualTree.CloneTree(rootVisualElement);
      var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/Styles.uss");
      rootVisualElement.styleSheets.Add(styleSheet);

      _container = rootVisualElement.Q<VisualElement>("Container");
      _filterStringTextField = rootVisualElement.Q<TextField>("FilterStringTextField");
      _searchButton = rootVisualElement.Q<Button>("SearchButton");
      _searchButton.clickable.clicked += OnSearchButtonClicked;
      _header = rootVisualElement.Q<VisualElement>("Header");
    }

    private void CreateSourcesControl(List<string> sources, string selected)
    {
      _sourcesControl = new PopupField<string>(sources, 2);
      _sourcesControl.value = selected;
      _sourcesControl.AddToClassList("SourcesControl");
      _header.Add(_sourcesControl);
      _sourcesControl.RegisterValueChangedCallback(OnSourcesControlValueChanged);
    }
    
    private async void OnSearchButtonClicked()
    {
      _container.Clear();
      var packages = await _nugetService.GetPackagesAsync(_filterStringTextField.text, 0, 20, CancellationToken.None);

      foreach (var package in packages)
      {
        var control = new PackageControl(_container, package, _nugetService);
        await control.SetFields();
      }
    }

    private void OnSourcesControlValueChanged(ChangeEvent<string> args) => _nugetService.ChangeActiveSource(args.newValue);
  }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = NuGet.Common.ILogger;

namespace Rdds.Unity.Nuget.UI
{
  public class MainWindow : EditorWindow
  {
    private ILogger _logger = null!;
    private INugetService _nugetService = null!;
    private FileService _fileService = null!;
    private NugetConfigService _nugetConfigService = null!;
    private FrameworkService _frameworkService = null!;
    
    private VisualElement _container = null!;
    private TextField _filterStringTextField = null!;
    private Button _searchButton = null!;
    private VisualElement _header = null!;
    private PopupField<string> _sourcesControl = null!;

    private CancellationTokenSource? _searchCancellationTokenSource;
    
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
      _frameworkService = new FrameworkService();
      _logger = new UnityConsoleLogger();
      _nugetService = new NugetService(_logger, _nugetConfigService, _fileService, _frameworkService);
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
      _sourcesControl = new PopupField<string>(sources, 0);
      _sourcesControl.value = selected;
      _sourcesControl.AddToClassList("SourcesControl");
      _header.Add(_sourcesControl);
      _sourcesControl.RegisterValueChangedCallback(OnSourcesControlValueChanged);
    }
    
    private async void OnSearchButtonClicked()
    {
      _container.Clear();
      _searchCancellationTokenSource?.Cancel();
      _searchCancellationTokenSource = new CancellationTokenSource();
      var packages = 
        await _nugetService.SearchPackagesAsync(_filterStringTextField.text, 0, 20, _searchCancellationTokenSource.Token);

      foreach (var package in packages)
      {
        if (_searchCancellationTokenSource.IsCancellationRequested)
          return;
        
        var control = new PackageControl(_container, package, _nugetService, _logger);
        await control.UpdateFields();
      }
    }

    private void OnSourcesControlValueChanged(ChangeEvent<string> args) => _nugetService.ChangeActiveSource(args.newValue);
  }
}
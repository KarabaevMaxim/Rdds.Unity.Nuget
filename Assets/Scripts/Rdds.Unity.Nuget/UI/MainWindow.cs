using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    #region Dependencies

    private ILogger _logger = null!;
    private INugetService _nugetService = null!;
    private FileService _fileService = null!;
    private NugetConfigService _nugetConfigService = null!;
    private FrameworkService _frameworkService = null!;
    private PackagesFileService _packagesFileService = null!;
    private InstalledPackagesService _installedPackagesService = null!;

    #endregion

    #region Controls
    
    private VisualElement _searchTabPanel = null!;
    private VisualElement _tabsContainer = null!;
    private Button _installedTabButton = null!;
    private Button _searchTabButton = null!;
    private VisualElement _packagesContainer = null!;
    private TextField _filterStringTextField = null!;
    private Button _searchButton = null!;
    private VisualElement _header = null!;
    private PopupField<string> _sourcesControl = null!;

    private InstalledTabControl _installedTab = null!;

    #endregion

    #region Fields

    private CancellationTokenSource? _searchCancellationTokenSource;

    #endregion

    #region Unity methods
    
    [MenuItem("Rdds/Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }
    
    private async void OnEnable()
    {
      InitializeDependencies();
      InitializeLayout();
      
      _nugetConfigService.LoadConfigFile();
      await _packagesFileService.LoadPackagesFile();
      CreateSourcesControl(_nugetConfigService.GetAvailableSources().ToList(), _nugetService.SelectedSource.Key);
      await OpenInstalledTabAsync();
    }

    #endregion

    private void InitializeDependencies()
    {
      _fileService = new FileService();
      _nugetConfigService = new NugetConfigService(_fileService);
      _frameworkService = new FrameworkService();
      _logger = new UnityConsoleLogger();
      _nugetService = new NugetService(_logger, _nugetConfigService, _fileService, _frameworkService);
      _packagesFileService = new PackagesFileService(_fileService, _logger);
      _installedPackagesService = new InstalledPackagesService(_packagesFileService, _nugetService, _nugetConfigService, _logger);
    }

    private void InitializeLayout()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.MainWindowLayout);
      visualTree.CloneTree(rootVisualElement);
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.Styles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.CommonStyles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.TabStyles));

      _packagesContainer = rootVisualElement.Q<VisualElement>("PackagesContainer");
      _filterStringTextField = rootVisualElement.Q<TextField>("FilterStringTextField");
      _searchButton = rootVisualElement.Q<Button>("SearchButton");
      _searchButton.clickable.clicked += OnSearchButtonClicked;
      _header = rootVisualElement.Q<VisualElement>("Header");

      _installedTab = new InstalledTabControl(
        rootVisualElement.Q<VisualElement>("InstalledTabPanel"),
        _packagesFileService, _nugetService, _installedPackagesService, _logger);

      _searchTabPanel = rootVisualElement.Q<VisualElement>("SearchTabPanel");
      _installedTabButton = rootVisualElement.Q<Button>("InstalledTabButton");
      _installedTabButton.clickable.clicked += OnInstalledButtonTabClicked;
      _searchTabButton = rootVisualElement.Q<Button>("SearchTabButton");
      _searchTabButton.clickable.clicked += OnSearchButtonTabClicked;
      _tabsContainer = rootVisualElement.Q<VisualElement>("TabsContainer");
    }

    private void CreateSourcesControl(List<string> sources, string selected)
    {
      _sourcesControl = new PopupField<string>(sources, 0);
      _sourcesControl.value = selected;
      _sourcesControl.AddToClassList("SourcesControl");
      _header.Add(_sourcesControl);
      _sourcesControl.RegisterValueChangedCallback(OnSourcesControlValueChanged);
    }

    private async Task OpenInstalledTabAsync()
    {
      if (_installedTab.Selected)
        return;
      
      _installedTab.Selected = true;
      _tabsContainer.Remove(_searchTabPanel);
      _installedTabButton.AddToClassList("OpenedTabButton");
      _searchTabButton.RemoveFromClassList("OpenedTabButton");
      await _installedTab.InitializeAsync();
    }

    private void OpenSearchTab()
    {
      // todo SearchTabControl.Selected required
      if (!_installedTab.Selected)
        return;
      
      _installedTab.Selected = false;
      _tabsContainer.Add(_searchTabPanel);
      _installedTabButton.RemoveFromClassList("OpenedTabButton");
      _searchTabButton.AddToClassList("OpenedTabButton");
    }
    
    #region Controls' event handlers

    private async void OnSearchButtonClicked()
    {
      _packagesContainer.Clear();
      _searchCancellationTokenSource?.Cancel();
      _searchCancellationTokenSource = new CancellationTokenSource();
      var packages = 
        await _nugetService.SearchPackagesAsync(_filterStringTextField.text, 0, 20, _searchCancellationTokenSource.Token);

      foreach (var package in packages)
      {
        if (_searchCancellationTokenSource.IsCancellationRequested)
          return;
        
        var control = new PackageControl(_packagesContainer, package, _nugetService, _logger);
        await control.InitializeAsync();
      }
    }
    
    private void OnSourcesControlValueChanged(ChangeEvent<string> args) => _nugetService.ChangeActiveSource(args.newValue);

    private async void OnInstalledButtonTabClicked() => await OpenInstalledTabAsync();
    
    private void OnSearchButtonTabClicked() => OpenSearchTab();

    #endregion
  }
}
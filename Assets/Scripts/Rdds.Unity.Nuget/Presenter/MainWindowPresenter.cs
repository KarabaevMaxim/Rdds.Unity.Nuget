using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Services.Configs;
using Rdds.Unity.Nuget.UI;
using Rdds.Unity.Nuget.UI.Controls.Models;

namespace Rdds.Unity.Nuget.Presenter
{
  internal class MainWindowPresenter
  {
    private const string AllAssemblies = "All assemblies";
    private const string AllSources = "All sources";

    #region Fields and properties

    private readonly IMainWindow _mainWindow;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;
    private readonly NugetConfigService _nugetConfigService;
    private readonly AssembliesService _assembliesService;
    private readonly RemotePackagesService _remotePackagesService;

    private readonly InstalledPackagesPresenter _installedPackagesPresenter;
    private readonly AvailablePackagesPresenter _availablePackagesPresenter;
    private readonly PackageDetailsPresenter _packageDetailsPresenter;
    
    private CancellationTokenSource? _filterByStringDelayCancellationTokenSource;

    private PackageRowPresentationModel? _lastSelectedPackageRow;

    private bool IsLoading
    {
      set => _mainWindow.IsListsLoading = value;
    }
    
    #endregion

    public async Task InitializeAsync()
    {
      Func<Task> action = async () =>
      {
        await Task.WhenAll(_installedPackagesConfigService.LoadConfigFileAsync(),
          _localPackagesConfigService.LoadConfigFileAsync(), 
          _nugetConfigService.LoadConfigFileAsync());
      
        _remotePackagesService.InitializeSources();
      
        InitializeSources();
        await InitializeAssembliesAsync();

        await Task.WhenAll(_installedPackagesPresenter.InitializeAsync(), _availablePackagesPresenter.InitializeAsync());
      };
      await StartLoadingOperationAsync(action);
    }
    
    private async void SelectPackageRowAsync(PackageRowPresentationModel selected)
    {
      if (_lastSelectedPackageRow.HasValue && _lastSelectedPackageRow.Value.Id == selected.Id)
        return;
      
      _lastSelectedPackageRow = selected;
      await _packageDetailsPresenter.ChangeSelectedPackageRowAsync(selected);
    }

    #region Filtration methods

    private async void FilterByIdAsync(string idPart)
    {
      _filterByStringDelayCancellationTokenSource?.Cancel();
      _filterByStringDelayCancellationTokenSource = new CancellationTokenSource();
      
      try
      {
        // wait for 0.5 sec after last input
        await Task.Delay(TimeSpan.FromSeconds(0.5f), _filterByStringDelayCancellationTokenSource.Token).ConfigureAwait(true);
      }
      catch (TaskCanceledException)
      {
        return;
      }
      
      await StartLoadingOperationAsync(() => Task.WhenAll(
        _installedPackagesPresenter.FilterByIdAsync(idPart),
        _availablePackagesPresenter.FilterByIdAsync(idPart)));
    }
    
    private async void FilterByAssembly(string assembly) => 
      await StartLoadingOperationAsync(() => _installedPackagesPresenter.FilterByAssemblyAsync(assembly == AllAssemblies ? null : assembly));

    private async void FilterBySourceAsync(string source)
    {
      var filterSource = source == AllSources ? null : source;
      await StartLoadingOperationAsync(() => 
        Task.WhenAll(_installedPackagesPresenter.FilterBySourceAsync(filterSource), _availablePackagesPresenter.FilterBySourceAsync(filterSource)));
    }

    #endregion

    private void InitializeSources()
    {
      var sources = new List<string> { AllSources };
      sources.AddRange(_nugetConfigService.RequireAvailableSourcesKeys());
      _mainWindow.Sources = sources;
      _mainWindow.SetSource(_remotePackagesService.SelectedSource?.Key ?? AllSources);
    }

    private async Task InitializeAssembliesAsync()
    {
      var assemblies = new List<string> { AllAssemblies };
      assemblies.AddRange((await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name));
      _mainWindow.Assemblies = assemblies;
    }

    private async Task StartLoadingOperationAsync(Func<Task> action)
    {
      IsLoading = true;
      await action.Invoke();
      IsLoading = false;
    }
    
    private async void InstalledPackagesListChangedAsync() => await _installedPackagesPresenter.InitializeAsync();

    public MainWindowPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService,
      InstalledPackagesConfigService installedPackagesConfigService, 
      LocalPackagesConfigService localPackagesConfigService,
      NugetConfigService nugetConfigService,
      AssembliesService assembliesService,
      RemotePackagesService remotePackagesService,
      FrameworkService frameworkService)
    {
      _mainWindow = mainWindow;
      _installedPackagesConfigService = installedPackagesConfigService;
      _localPackagesConfigService = localPackagesConfigService;
      _nugetConfigService = nugetConfigService;
      _assembliesService = assembliesService;
      _remotePackagesService = remotePackagesService;
      _installedPackagesPresenter = new InstalledPackagesPresenter(_mainWindow, localPackagesService, _remotePackagesService, _installedPackagesConfigService);
      _availablePackagesPresenter = new AvailablePackagesPresenter(_mainWindow, _remotePackagesService, _installedPackagesConfigService);
      _packageDetailsPresenter = new PackageDetailsPresenter(_mainWindow, localPackagesService, _installedPackagesConfigService, _remotePackagesService, _assembliesService, frameworkService);
      
      _mainWindow.PackageRowSelected += SelectPackageRowAsync; 
      _mainWindow.FilterTextChanged += FilterByIdAsync;
      _mainWindow.AssemblyChanged += FilterByAssembly;
      _mainWindow.SourceChanged += FilterBySourceAsync;

      _packageDetailsPresenter.PackageInstalledOrRemoved += InstalledPackagesListChangedAsync;
    }
  }
}
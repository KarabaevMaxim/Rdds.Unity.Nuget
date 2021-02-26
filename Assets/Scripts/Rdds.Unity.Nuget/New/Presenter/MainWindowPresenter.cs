using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Services;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class MainWindowPresenter
  {
    private const string AllAssemblies = "All assemblies";
    private const string AllSources = "All sources";

    #region Fields

    private readonly IMainWindow _mainWindow;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;
    private readonly NugetConfigService _nugetConfigService;
    private readonly AssembliesService _assembliesService;
    private readonly RemotePackagesService _remotePackagesService;

    private readonly InstalledPackagesPresenter _installedPackagesPresenter;
    private readonly AvailablePackagesPresenter _availablePackagesPresenter;
    private readonly PackageDetailsPresenter _packageDetailsPresenter;

    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
    private CancellationTokenSource? _filterByStringDelayCancellationTokenSource;
    
    #endregion

    public async Task InitializeAsync()
    {
      await Task.WhenAll(_installedPackagesConfigService.LoadConfigFileAsync(),
        _localPackagesConfigService.LoadConfigFileAsync(), 
        _nugetConfigService.LoadConfigFileAsync());
      
      _remotePackagesService.InitializeSources();
      
      InitializeSources();
      await InitializeAssembliesAsync();

      await Task.WhenAll(_installedPackagesPresenter.InitializeAsync(), _availablePackagesPresenter.InitializeAsync());
    }
    
    private void SelectPackageRow(PackageRowPresentationModel selected)
    {
      _packageDetailsPresenter.Reset();
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();
      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version)); 
      // var installed = EditorContext.InstalledPackagesService.IsPackageInstalled(selected.Id);
      // var installIcon = installed
      //   ? ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName)
      //   : ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName);
      // Action installAction = () => { };
      // Action? updateAction = installed && !EditorContext.InstalledPackagesService.EqualInstalledPackageVersion(identity)
      //                    ? () => { }
      //                    : (Action?)null;
      //
      // var assemblies = _assemblies
      //   .Select(a =>
      //     new AssemblyPackageDetailsPresentationModel(ImageHelper.LoadBuiltinImage("AssemblyDefinitionAsset Icon"),
      //       a.Name, null, ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName), () => { }))
      //   .ToList();
      // var details = new PackageDetailsPresentationModel(selected.Id, selected.Icon, selected.Version,
      //   new List<string> {selected.Version}, selected.Sources.First(), selected.Sources, null,
      //   null, installIcon, installAction, updateAction, assemblies);
      // _packageDetailsControl.Details = details;
      //
      // var versions = await EditorContext.NugetService.GetPackageVersionsAsync(selected.Id, _loadDetailsCancellationTokenSource.Token);
      // details.Versions = versions.Select(v => v.ToString()).ToList();
      // // delayed adding versions
      // _packageDetailsControl.Details = details;
      //
      // var detailInfo = await EditorContext.NugetService.GetPackageAsync(identity, _loadDetailsCancellationTokenSource.Token);
      //
      // if (detailInfo == null)
      // {
      //   LogHelper.LogWarning($"Package {identity.Id} with version {identity.Version} not found in online sources");
      //   return;
      // }
      //
      // details.Dependencies = detailInfo.Dependencies?
      // .Select(d => new DependenciesPresentationModel(d.TargetFramework.Name, d.Dependencies
      //   .Select(dd => new DependencyPresentationModel(dd.Id, dd.Version.ToString()))));
      // details.Description = detailInfo.Description;
      //
      // // delayed adding dependencies and description
      // _packageDetailsControl.Details = details;
    }

    #region Filtration methods

    private async void FilterByIdAsync(string idPart)
    {
      _filterByStringDelayCancellationTokenSource?.Cancel();
      _filterByStringDelayCancellationTokenSource = new CancellationTokenSource();
      
      try
      {
        // wait for 1 sec after last input
        await Task.Delay(1000, _filterByStringDelayCancellationTokenSource.Token).ConfigureAwait(true);
      }
      catch (TaskCanceledException)
      {
        return;
      }
      
      //todo check it with ConfigureAwait(true/false)
      // args.newValue not working with async method
      _installedPackagesPresenter.FilterById(idPart);
      await _availablePackagesPresenter.FilterByIdAsync(idPart);
    }
    
    private void FilterByAssembly(string assembly) => 
      _installedPackagesPresenter.FilterByAssembly(assembly == AllAssemblies ? null : assembly);

    private async void FilterBySourceAsync(string source)
    {
      _installedPackagesPresenter.FilterBySource(source == AllSources ? null : source);
      await _availablePackagesPresenter.FilterBySourceAsync(source == AllSources ? null : source);
    }

    #endregion

    private void InitializeSources()
    {
      var sources = new List<string> { AllSources };
      sources.AddRange(_nugetConfigService.RequireAvailableSourcesKeys());
      _mainWindow.Sources = sources;
    }

    private async Task InitializeAssembliesAsync()
    {
      var assemblies = new List<string> { AllAssemblies };
      assemblies.AddRange((await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name));
      _mainWindow.Assemblies = assemblies;
    }

    public MainWindowPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService,
      InstalledPackagesConfigService installedPackagesConfigService, 
      LocalPackagesConfigService localPackagesConfigService,
      NugetConfigService nugetConfigService,
      AssembliesService assembliesService,
      RemotePackagesService remotePackagesService)
    {
      _mainWindow = mainWindow;
      _installedPackagesConfigService = installedPackagesConfigService;
      _localPackagesConfigService = localPackagesConfigService;
      _nugetConfigService = nugetConfigService;
      _assembliesService = assembliesService;
      _remotePackagesService = remotePackagesService;
      _installedPackagesPresenter = new InstalledPackagesPresenter(_mainWindow, localPackagesService, _installedPackagesConfigService);
      _availablePackagesPresenter = new AvailablePackagesPresenter(_mainWindow, _remotePackagesService, _installedPackagesConfigService);
      _packageDetailsPresenter = new PackageDetailsPresenter(_mainWindow.PackageDetailsControl);
      
      _mainWindow.PackageRowSelected += SelectPackageRow; 
      _mainWindow.FilterTextChanged += FilterByIdAsync;
      _mainWindow.AssemblyChanged += FilterByAssembly;
      _mainWindow.SourceChanged += FilterBySourceAsync;
    }
  }
}
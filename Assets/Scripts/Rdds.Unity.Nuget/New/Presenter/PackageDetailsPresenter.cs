using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class PackageDetailsPresenter
  {
    #region Fields and properties

    private readonly IMainWindow _mainWindow;
    private readonly LocalPackagesService _localPackagesService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly RemotePackagesService _remotePackagesService;
    private readonly AssembliesService _assembliesService;
    private readonly FrameworkService _frameworkService;

    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
    private CancellationTokenSource? _installCancellationTokenSource;
    private CancellationTokenSource? _removeCancellationTokenSource;

    private PackageRowPresentationModel? _selectedPackage;
    
    private string? SelectedSource => _mainWindow.DetailsSelectedSource;
    
    private string? SelectedVersion => _mainWindow.DetailsSelectedVersion;

    private bool IsLoading
    {
      get => _mainWindow.IsDetailsLoading;
      set => _mainWindow.IsDetailsLoading = value;
    }

    #endregion

    public async Task ChangeSelectedPackageRowAsync(PackageRowPresentationModel selected)
    {
      _selectedPackage = selected;
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();

      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version));
      
      var installed = _localPackagesService.IsPackageInstalled(selected.Id);
      var installIcon = installed
        ? ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName)
        : ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName);
      Action installAction = installed ? RemovePackageAsync : (Action)InstallPackageAsync;
      Action? updateAction = installed
                         ? () => { }
                         : (Action?)null;

      var assemblies = _installedPackagesConfigService.GetInstalledInAssemblies(selected.Id)?
        .Select(a =>
        {
          var icon = ImageHelper.LoadBuiltinImage(Paths.AssemblyDefinitionAssetIconBuiltInResourceName);
          var buttonIcon = ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName);
          Action buttonAction = () => { };
          return new AssemblyPackageDetailsPresentationModel(icon, a, null, buttonIcon, buttonAction);
        }) ?? new AssemblyPackageDetailsPresentationModel[0];

      var selectedDetails = new PackageDetailsPresentationModel(selected.Id, selected.Icon, selected.Version,
        new[] { selected.Version }, selected.Sources.FirstOrDefault() ?? string.Empty, selected.Sources, null,
        null, installIcon, installAction, updateAction, assemblies);
      _mainWindow.SelectedPackage = selectedDetails;

      if (!string.IsNullOrWhiteSpace(selectedDetails.SelectedSource))
      {
        // delayed adding versions
        await SetVersionsAsync(selectedDetails, selected, _loadDetailsCancellationTokenSource.Token);
        PackageInfoSourceWrapper? detailInfo = null;
      
        try
        {
          detailInfo =  await _remotePackagesService.GetPackageInfoAsync(identity, selectedDetails.SelectedSource,
            _loadDetailsCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
        }
      
        if (!detailInfo.HasValue)
        {
          LogHelper.LogWarning($"Package {identity.Id} with version {identity.Version} not found in online sources");
          return;
        }
      
        // delayed adding dependencies and description
        SetDependencies(selectedDetails, detailInfo.Value);
      }
    }

    private async void InstallPackageAsync()
    {
      IsLoading = true;

      try
      {
        _installCancellationTokenSource?.Cancel();
        _installCancellationTokenSource = new CancellationTokenSource();
        var identity = new PackageIdentity(_selectedPackage!.Value.Id, SelectedVersion!);
      
        var task = Task.Run(async () =>
        {
          try
          {
            await _remotePackagesService.DownloadPackageAsync(identity, SelectedSource!, _installCancellationTokenSource.Token);
          }
          catch (TaskCanceledException)
          {
            return true;
          }
      
          var assemblies = (await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name);
          var framework = _frameworkService.RequireCurrentFramework();
          return await _localPackagesService.InstallPackageAsync(identity, assemblies, framework);
        }, _installCancellationTokenSource.Token);
      
        var successInstall = await DialogHelper.ShowLoadingAsync("Installing", "Please wait while package installing...", task);

        if (!successInstall) 
          DialogHelper.ShowErrorAlert($"Failed to install package {identity}");

        await ChangeSelectedPackageRowAsync(_selectedPackage!.Value);
      }
      finally
      {
        IsLoading = false;
      }
    }
    
    private async void RemovePackageAsync()
    {
      IsLoading = true;

      try
      {
        _removeCancellationTokenSource?.Cancel();
        _removeCancellationTokenSource = new CancellationTokenSource();

        var removeTask = Task.Run(async () =>
        {
          var assemblies = (await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name);
          return await _localPackagesService.RemovePackageAsync(_selectedPackage!.Value.Id, assemblies);
        }, _removeCancellationTokenSource.Token);
      
        var successRemove = await DialogHelper.ShowLoadingAsync("Removing", "Please wait while package removing...", removeTask);
      
        if (!successRemove) 
          DialogHelper.ShowErrorAlert($"Failed to install package {_selectedPackage!.Value.Id}");
      }
      finally
      {
        IsLoading = false;
      }
    }

    private async Task SetVersionsAsync(PackageDetailsPresentationModel details, PackageRowPresentationModel selectedModel, CancellationToken cancellationToken)
    {
      var versions = 
        await _remotePackagesService.FindPackageVersionsAsync(selectedModel.Id, selectedModel.Sources.First(), cancellationToken);
      details.Versions = versions.Select(v => v.PackageVersion.ToString());
      _mainWindow.SelectedPackage = details;
    }
    
    private void SetDependencies(PackageDetailsPresentationModel details, PackageInfoSourceWrapper detailInfo)
    {
      details.Dependencies = detailInfo.PackageInfo.Dependencies?
        .Select(d => new DependenciesPresentationModel(d.TargetFramework.Name, d.Dependencies
          .Select(dd => new DependencyPresentationModel(dd.Id, dd.Version.ToString()))));
      details.Description = detailInfo.PackageInfo.Description;
      
      _mainWindow.SelectedPackage = details;
    }
    
    #region Constructor

    public PackageDetailsPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService, 
      InstalledPackagesConfigService installedPackagesConfigService,
      RemotePackagesService remotePackagesService,
      AssembliesService assembliesService,
      FrameworkService frameworkService)
    {
      _mainWindow = mainWindow;
      _localPackagesService = localPackagesService;
      _installedPackagesConfigService = installedPackagesConfigService;
      _remotePackagesService = remotePackagesService;
      _assembliesService = assembliesService;
      _frameworkService = frameworkService;
    }

    #endregion
  }
}
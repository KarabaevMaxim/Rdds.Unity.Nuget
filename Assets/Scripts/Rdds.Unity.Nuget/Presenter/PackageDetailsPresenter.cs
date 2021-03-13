using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.UI;
using Rdds.Unity.Nuget.UI.Controls.Models;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.Presenter
{
  internal class PackageDetailsPresenter
  {
    #region Fields and properties

    private readonly IMainWindow _mainWindow;
    private readonly LocalPackagesService _localPackagesService;
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
      set
      {
        _mainWindow.IsDetailsLoading = value;

        // disabled because it calling some bugs with manual refreshing asset database.
        // if (value)
        //   AssetDatabase.StartAssetEditing();
        // else
        //   AssetDatabase.StopAssetEditing();
      }
    }

    #endregion

    #region Events

    public event Action? PackageInstalledOrRemoved;

    #endregion

    public async Task ChangeSelectedPackageRowAsync(PackageRowPresentationModel selected)
    {
      _selectedPackage = selected;
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();

      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version));
      
      var installed = _localPackagesService.IsPackageInstalled(selected.Id);
      var installIcon = RequireInstallOrRemoveIcon(installed);
      Action installAction = RequireInstallOrRemoveAction(installed);
      
      var assemblies = (await _assembliesService.RequireAllAssembliesAsync())
        .Select(assembly =>
        {
          var icon = ImageHelper.LoadImageFromResource(Paths.AssemblyDefinitionAssetIconResourceName);
          var installedInAssembly = _assembliesService.IsPackageInstalledInAssembly(selected.Id, assembly.Name);
          var version = installedInAssembly
            ? _assembliesService.RequireInstalledPackageVersion(selected.Id, assembly.Name)
            : null;
          var buttonIcon = RequireInstallOrRemoveIcon(installedInAssembly);
          // ReSharper disable once ConvertToLocalFunction
          Action buttonAction = RequireInstallOrRemoveAction(installedInAssembly, new[] { assembly.Name });
          return new AssemblyPackageDetailsPresentationModel(icon, assembly.Name, version, buttonIcon, buttonAction);
        });

      var selectedDetails = new PackageDetailsPresentationModel(selected.Id, selected.IsInstalled, selected.Icon, selected.Version,
        new[] { selected.Version }, selected.Sources.FirstOrDefault() ?? string.Empty, selected.Sources, null,
        null, installIcon, installAction, null, assemblies);
      _mainWindow.SelectedPackage = selectedDetails;

      if (!string.IsNullOrWhiteSpace(selectedDetails.SelectedSource))
      {
        // delayed adding versions
        await SetVersionsAsync(selectedDetails, selected, _loadDetailsCancellationTokenSource.Token);
        PackageInfoSourceWrapper? detailInfo;
      
        try
        {
          detailInfo =  await _remotePackagesService.GetPackageInfoAsync(identity, selectedDetails.SelectedSource,
            _loadDetailsCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
          return;
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

    private async Task InstallPackageAsync(IEnumerable<string>? assemblyNames)
    {
      try
      {
        IsLoading = true;
        _installCancellationTokenSource?.Cancel();
        _installCancellationTokenSource = new CancellationTokenSource();
        var identity = new PackageIdentity(_selectedPackage!.Value.Id, SelectedVersion!);
      
        var task = Task.Run(async () =>
        {
          try
          {
            await _remotePackagesService.DownloadPackageAsync(identity, SelectedSource!, false, _installCancellationTokenSource.Token);
          }
          catch (TaskCanceledException)
          {
            return true;
          }
      
          var assemblies = assemblyNames ?? (await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name);
          var framework = _frameworkService.RequireCurrentFramework();
          return await _localPackagesService.InstallPackageAsync(identity, assemblies, framework);
        }, _installCancellationTokenSource.Token);
      
        var successInstall = await DialogHelper.ShowLoadingAsync("Installing", "Please wait while package installing...", task);

        if (successInstall)
        {
          PackageInstalledOrRemoved?.Invoke();
          await ChangeSelectedPackageRowAsync(_selectedPackage!.Value);
        }
        else
          DialogHelper.ShowErrorAlert($"Failed to install package {identity}");
      }
      finally
      {
        IsLoading = false;
      }
    }
    
    private async Task RemovePackageAsync(IEnumerable<string>? assemblyNames)
    {
      try
      {
        IsLoading = true;
        _removeCancellationTokenSource?.Cancel();
        _removeCancellationTokenSource = new CancellationTokenSource();

        var removeTask = Task.Run(async () =>
        {
          var assemblies = assemblyNames ?? (await _assembliesService.RequireAllAssembliesAsync()).Select(a => a.Name);
          return await _localPackagesService.RemovePackageAsync(_selectedPackage!.Value.Id, assemblies);
        }, _removeCancellationTokenSource.Token);
      
        var successRemove = await DialogHelper.ShowLoadingAsync("Removing", "Please wait while package removing...", removeTask);

        if (successRemove)
        {
          PackageInstalledOrRemoved?.Invoke();
          await ChangeSelectedPackageRowAsync(_selectedPackage!.Value);
        }
        else
          DialogHelper.ShowErrorAlert($"Failed to remove package {_selectedPackage!.Value.Id}");
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

    private Texture2D RequireInstallOrRemoveIcon(bool packageInstalled)
    {
      return packageInstalled
        ? ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName)
        : ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName);
    }

    private Action RequireInstallOrRemoveAction(bool packageInstalled, IEnumerable<string>? assemblies = null)
    {
      return packageInstalled 
        ? async () => await RemovePackageAsync(assemblies) : 
        (Action)(async () => await InstallPackageAsync(assemblies));
    }
    
    #region Constructor

    public PackageDetailsPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService,
      RemotePackagesService remotePackagesService,
      AssembliesService assembliesService,
      FrameworkService frameworkService)
    {
      _mainWindow = mainWindow;
      _localPackagesService = localPackagesService;
      _remotePackagesService = remotePackagesService;
      _assembliesService = assembliesService;
      _frameworkService = frameworkService;
    }

    #endregion
  }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class PackageDetailsPresenter
  {
    private readonly IMainWindow _mainWindow;
    private readonly LocalPackagesService _localPackagesService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly RemotePackagesService _remotePackagesService;

    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
    
    public async Task ChangeSelectedPackageRowAsync(PackageRowPresentationModel selected)
    {
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();
      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version));
      
      var installed = _localPackagesService.IsPackageInstalled(selected.Id);
      var installIcon = installed
        ? ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName)
        : ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName);
      Action installAction = () => { };
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
        new[] { selected.Version }, selected.Sources.First(), selected.Sources, null,
        null, installIcon, installAction, updateAction, assemblies);
      _mainWindow.SelectedPackage = selectedDetails;
      
      var versions = 
        await _remotePackagesService.FindPackageVersionsAsync(selected.Id, selected.Sources.First(), _loadDetailsCancellationTokenSource.Token);
      selectedDetails.Versions = versions.Select(v => v.PackageVersion.ToString());
      // delayed adding versions
      _mainWindow.SelectedPackage = selectedDetails;

      PackageInfoSourceWrapper? detailInfo = null;
      
      try
      {
        detailInfo = await _remotePackagesService.GetPackageInfoAsync(identity, selected.Sources.First(), _loadDetailsCancellationTokenSource.Token);
      }
      catch (TaskCanceledException)
      {
      }
      
      if (!detailInfo.HasValue)
      {
        LogHelper.LogWarning($"Package {identity.Id} with version {identity.Version} not found in online sources");
        return;
      }
      
      selectedDetails.Dependencies = detailInfo.Value.PackageInfo.Dependencies?
      .Select(d => new DependenciesPresentationModel(d.TargetFramework.Name, d.Dependencies
        .Select(dd => new DependencyPresentationModel(dd.Id, dd.Version.ToString()))));
      selectedDetails.Description = detailInfo.Value.PackageInfo.Description;
      // delayed adding dependencies and description
      _mainWindow.SelectedPackage = selectedDetails;
    }

    public PackageDetailsPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService, 
      InstalledPackagesConfigService installedPackagesConfigService,
      RemotePackagesService remotePackagesService)
    {
      _mainWindow = mainWindow;
      _localPackagesService = localPackagesService;
      _installedPackagesConfigService = installedPackagesConfigService;
      _remotePackagesService = remotePackagesService;
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class AvailablePackagesPresenter
  {
    private const int PageSize = 20;
    
    private readonly IMainWindow _mainWindow;
    private readonly RemotePackagesService _remotePackagesService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;

    private CancellationTokenSource? _loadPackagesCancellationTokenSource;
    private string _lastFilterString = null!;
    
    public async Task InitializeAsync()
    {
      _lastFilterString = string.Empty;
      await ReloadPackagesAsync();
    }

    public Task FilterByIdAsync(string idPart)
    {
      _lastFilterString = idPart;
      return ReloadPackagesAsync();
    }

    public Task FilterBySourceAsync(string? source)
    {
      _remotePackagesService.ChangeActiveSource(source);
      return ReloadPackagesAsync();
    }
    
    private async Task<PackageRowPresentationModel> CreatePresentationModelAsync(PackageInfoSourceWrapper packageInfo)
    {
      var id = packageInfo.PackageInfo.Identity.Id;
      var version = packageInfo.PackageInfo.Identity.Version.ToString();
      var icon = (packageInfo.PackageInfo.IconPath == null
                   ? Resources.Load<Texture>(Paths.DefaultIconResourceName)
                   : await ImageHelper.LoadImageAsync(packageInfo.PackageInfo.IconPath, CancellationToken.None)) 
                 ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName);
      var sources = packageInfo.SourceKeys;
      var assemblies = _installedPackagesConfigService.GetInstalledInAssemblies(id) ?? new List<string>(0);
      return new PackageRowPresentationModel(id, version, icon, sources, assemblies);
    }

    private async Task ReloadPackagesAsync()
    {
      _loadPackagesCancellationTokenSource?.Cancel();
      _loadPackagesCancellationTokenSource = new CancellationTokenSource();
      var tasks = (await _remotePackagesService.FindPackagesAsync(_lastFilterString, 0, PageSize, _loadPackagesCancellationTokenSource.Token))
        .Select(CreatePresentationModelAsync);
      var vms = await Task.WhenAll(tasks);
      _mainWindow.AvailablePackages = vms;
    }
    
    public AvailablePackagesPresenter(IMainWindow mainWindow, 
      RemotePackagesService remotePackagesService, 
      InstalledPackagesConfigService installedPackagesConfigService)
    {
      _mainWindow = mainWindow;
      _remotePackagesService = remotePackagesService;
      _installedPackagesConfigService = installedPackagesConfigService;
    }
  }
}
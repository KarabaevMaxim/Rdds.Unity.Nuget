using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Other;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Services.Configs;
using Rdds.Unity.Nuget.UI;
using Rdds.Unity.Nuget.UI.Controls.Models;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.Presenter
{
  internal class InstalledPackagesPresenter
  {
    private readonly IMainWindow _mainWindow;
    private readonly LocalPackagesService _localPackagesService;
    private readonly RemotePackagesService _remotePackagesService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    
    private readonly Dictionary<string, CancellationTokenSource?> _createModelCancellationTokenSources;
    private CancellationTokenSource? _reloadPackagesCancellationTokenSource;
    
    private string _lastFilterString = null!;
    private string? _lastSelectedAssembly;
    private string? _lastSelectedSource;

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
    
    public Task FilterByAssemblyAsync(string? assembly)
    {
      _lastSelectedAssembly = assembly;
      return ReloadPackagesAsync();
    }
    
    public Task FilterBySourceAsync(string? source)
    {
      _lastSelectedSource = source;
      return ReloadPackagesAsync();
    }

    private async Task<PackageRowPresentationModel> CreatePresentationModelAsync(PackageInfo packageInfo)
    {
      if (_createModelCancellationTokenSources.TryGetValue(packageInfo.Identity.Id, out var cancellationTokenSource))
        cancellationTokenSource?.Cancel();
      
      _createModelCancellationTokenSources[packageInfo.Identity.Id] = new CancellationTokenSource();
      var token = _createModelCancellationTokenSources[packageInfo.Identity.Id]!.Token;
      
      var id = packageInfo.Identity.Id;
      var version = packageInfo.Identity.Version.ToString();
      var icon = (packageInfo.IconPath == null
                   ? Resources.Load<Texture>(Paths.DefaultIconResourceName)
                   : await ImageHelper.LoadImageAsync(packageInfo.IconPath, token)) 
                 ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName);
      
      var assemblies = _installedPackagesConfigService.RequireInstalledInAssemblies(id);
      var sources = await _remotePackagesService.FindSourcesForPackageAsync(id, token);
      return new PackageRowPresentationModel(id, version, icon, sources, assemblies);
    }
    
    private async Task ReloadPackagesAsync()
    {
      _reloadPackagesCancellationTokenSource?.Cancel();
      _reloadPackagesCancellationTokenSource = new CancellationTokenSource();
      
      var identities = _localPackagesService.RequireInstalledPackages()
        .Where(i => i.Id.ContainsIgnoreCase(_lastFilterString));
      
      var tasks = identities.Select(async i =>
      {
        var packageInfo = await _localPackagesService.GetInstalledPackageInfoAsync(i.Id);

        if (packageInfo == null)
          return new PackageRowPresentationModel(i.Id, i.Version.ToString(), 
            Resources.Load<Texture>(Paths.DefaultIconResourceName), 
            Enumerable.Empty<string>(), 
            Enumerable.Empty<string>());

        return await CreatePresentationModelAsync(packageInfo);
      });

      // todo break method if _reloadPackagesCancellationTokenSource.Cancelled
      IEnumerable<PackageRowPresentationModel> packages = await Task.WhenAll(tasks);

      if (!string.IsNullOrWhiteSpace(_lastSelectedAssembly))
        packages = packages.Where(p => p.InstalledInAssemblies.Contains(_lastSelectedAssembly));

      if (!string.IsNullOrWhiteSpace(_lastSelectedSource))
        packages = packages.Where(p => p.Sources.Contains(_lastSelectedSource));

      _mainWindow.InstalledPackages = packages;
    }

    public InstalledPackagesPresenter(IMainWindow mainWindow, 
      LocalPackagesService localPackagesService, 
      RemotePackagesService remotePackagesService,
      InstalledPackagesConfigService installedPackagesConfigService)
    {
      _mainWindow = mainWindow;
      _localPackagesService = localPackagesService;
      _remotePackagesService = remotePackagesService;
      _installedPackagesConfigService = installedPackagesConfigService;
      _createModelCancellationTokenSources = new Dictionary<string, CancellationTokenSource?>();
    }
  }
}
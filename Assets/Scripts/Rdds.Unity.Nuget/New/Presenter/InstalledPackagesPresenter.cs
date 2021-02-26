using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Other;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class InstalledPackagesPresenter
  {
    private readonly IMainWindow _mainWindow;
    private readonly LocalPackagesService _localPackagesService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;

    private readonly List<PackageRowPresentationModel> _installedPackages;

    public async Task InitializeAsync()
    {
      var identities = _localPackagesService.RequireInstalledPackages();

      foreach (var identity in identities)
      {
        var packageInfo = await _localPackagesService.RequireInstalledPackageInfoAsync(identity.Id);
        var pm = await CreatePresentationModelAsync(packageInfo);
        _installedPackages.Add(pm);
      }
      
      _mainWindow.InstalledPackages = _installedPackages;
    }
    
    public void FilterById(string idPart)
    {
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.Id.ContainsIgnoreCase(idPart));
      _mainWindow.InstalledPackages = filtered;
    }
    
    public void FilterByAssembly(string? assembly)
    {
      // todo implement clear filter if assembly equals null
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.InstalledInAssemblies.Contains(assembly));
      _mainWindow.InstalledPackages = filtered;
    }
    
    public void FilterBySource(string? source)
    {
      // todo implement clear filter if source equals null
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.Sources.Contains(source));
      _mainWindow.InstalledPackages = filtered;
    }

    private async Task<PackageRowPresentationModel> CreatePresentationModelAsync(PackageInfo packageInfo)
    {
      var id = packageInfo.Identity.Id;
      var version = packageInfo.Identity.Version.ToString();
      var icon = (packageInfo.IconPath == null
                   ? Resources.Load<Texture>(Paths.DefaultIconResourceName)
                   : await ImageHelper.LoadImageAsync(packageInfo.IconPath, CancellationToken.None)) 
                 ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName);
      // todo take sources where package is available
      // Must we have this property?
      var sources = new List<string> { "Gitlab" };
      var assemblies = _installedPackagesConfigService.RequireInstalledInAssemblies(id);
      return new PackageRowPresentationModel(id, version, icon, sources, assemblies);
    }

    public InstalledPackagesPresenter(IMainWindow mainWindow, LocalPackagesService localPackagesService, InstalledPackagesConfigService installedPackagesConfigService)
    {
      _mainWindow = mainWindow;
      _localPackagesService = localPackagesService;
      _installedPackagesConfigService = installedPackagesConfigService;
      _installedPackages = new List<PackageRowPresentationModel>();
    }
  }
}
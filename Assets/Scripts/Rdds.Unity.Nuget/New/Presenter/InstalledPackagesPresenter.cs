using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Other;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class InstalledPackagesPresenter
  {
    private readonly IMainWindow _mainWindow;

    private IEnumerable<PackageRowPresentationModel> _installedPackages;

    public async Task InitializeAsync()
    {
      var installedPackagesService = EditorContext.InstalledPackagesService;
      var packagesFileService = EditorContext.PackagesFileService;
      var packages = installedPackagesService.RequireInstalledPackagesList();

      var tasks = packages.Select(async p =>
      {
        var icon = (p.IconPath == null
          ? Resources.Load<Texture>(Paths.DefaultIconResourceName)
          : await ImageHelper.LoadImageAsync(p.IconPath, CancellationToken.None)) 
                   ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName);
        var source = packagesFileService.RequirePackage(p.Identity.Id).Source;

        // todo initialize assemblies in which package installed
        return new PackageRowPresentationModel(
          p.Identity.Id, p.Identity.Version.ToString(), icon, new List<string> { source }, new List<string>());
      });

      var models = await Task.WhenAll(tasks);
      _installedPackages = models;
      _mainWindow.InstalledPackages = _installedPackages.ToList();
    }
    
    public void FilterById(string idPart)
    {
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.Id.ContainsIgnoreCase(idPart));
      _mainWindow.InstalledPackages = filtered.ToList();
    }
    
    public void FilterByAssembly(string? assembly)
    {
      // todo implement clear filter if assembly equals null
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.InstalledInAssemblies.Contains(assembly));
      _mainWindow.InstalledPackages = filtered.ToList();
    }
    
    public void FilterBySource(string? source)
    {
      // todo implement clear filter if source equals null
      var filtered = _mainWindow.InstalledPackages
        .Where(p => p.Sources.Contains(source));
      _mainWindow.InstalledPackages = filtered.ToList();
    }

    public InstalledPackagesPresenter(IMainWindow mainWindow) => _mainWindow = mainWindow;
  }
}
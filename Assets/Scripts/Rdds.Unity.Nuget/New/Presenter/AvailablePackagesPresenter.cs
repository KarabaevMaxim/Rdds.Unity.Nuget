using System.Collections.Generic;
using System.Linq;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using Rdds.Unity.Nuget.Other;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class AvailablePackagesPresenter
  {
    private readonly IMainWindow _mainWindow;

    public void Initialize()
    {
      _mainWindow.AvailablePackages = new List<PackageRowPresentationModel>();
    }
    
    public void FilterById(string idPart)
    {
      var filtered = _mainWindow.AvailablePackages
        .Where(p => p.Id.ContainsIgnoreCase(idPart));
      _mainWindow.AvailablePackages = filtered.ToList();
    }

    public void FilterBySource(string? source)
    {
      // todo implement clear filter if source equals null
      var filtered = _mainWindow.AvailablePackages
        .Where(p => p.Sources.Contains(source));
      _mainWindow.AvailablePackages = filtered.ToList();
    }
    
    public AvailablePackagesPresenter(IMainWindow mainWindow) => _mainWindow = mainWindow;
  }
}
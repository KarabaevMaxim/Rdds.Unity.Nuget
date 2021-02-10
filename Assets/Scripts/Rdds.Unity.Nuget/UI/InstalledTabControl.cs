using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using Rdds.Unity.Nuget.Services;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  public class InstalledTabControl
  {
    private readonly VisualElement _controlRoot;
    private readonly VisualElement _controlParent;
    private readonly VisualElement _packagesContainer;
    private readonly Label _emptyPlaceholderLabel;
    
    private readonly PackagesFileService _packagesFileService;
    private readonly INugetService _nugetService;
    private readonly InstalledPackagesService _installedPackagesService;
    private readonly ILogger _logger;

    private bool _selected;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public bool Selected
    {
      get => _selected;
      set
      {
        if (_selected == value)
          return;
        
        _selected = value;

        if (_selected)
          _controlParent.Add(_controlRoot);
        else
          _controlParent.Remove(_controlRoot);
      }
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task InitializeAsync()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();

      // var installedPackages = _packagesFileService.RequirePackages();
      //
      // if (!installedPackages.Any())
      // {
      //   _controlRoot.Add(_emptyPlaceholderLabel);
      //   return;
      // }
      //
      // _controlRoot.Remove(_emptyPlaceholderLabel);
      //
      // foreach (var identity in installedPackages)
      // {
      //   if (_cancellationTokenSource.IsCancellationRequested)
      //     return;
      //   
      //   var package = await _nugetService.GetPackageAsync(identity, _cancellationTokenSource.Token);
      //
      //   if (package == null)
      //   {
      //     _logger.LogWarning($"Package {identity.Id} with version {identity.Version} not found");
      //     continue;
      //   }
      //
      //   var control = new PackageControl(_packagesContainer, package, _nugetService, _logger);
      //   await control.InitializeAsync();
      // }

      var installedPackages =
        await _installedPackagesService.UpdateInstalledPackagesListAsync(_cancellationTokenSource.Token);

      if (!installedPackages.Any())
      {
        if (!_controlRoot.Children().Contains(_emptyPlaceholderLabel)) 
          _controlRoot.Add(_emptyPlaceholderLabel);

        return;
      }
      
      _controlRoot.Remove(_emptyPlaceholderLabel);
      
      foreach (var package in installedPackages)
      {
        if (_cancellationTokenSource.IsCancellationRequested)
          return;
        
        var control = new PackageControl(_packagesContainer, package, _nugetService, _logger);
        await control.InitializeAsync();
      }
    }
    
    public InstalledTabControl(VisualElement controlRoot, PackagesFileService packagesFileService, INugetService nugetService, InstalledPackagesService installedPackagesService, ILogger logger)
    {
      _controlRoot = controlRoot;
      _controlParent = _controlRoot.parent;
      _packagesFileService = packagesFileService;
      _nugetService = nugetService;
      _installedPackagesService = installedPackagesService;
      _logger = logger;

      _packagesContainer = _controlRoot.Q<VisualElement>("InstalledPackagesContainer");
      _emptyPlaceholderLabel = _controlRoot.Q<Label>("EmptyPlaceholderLabel");
    }
  }
}
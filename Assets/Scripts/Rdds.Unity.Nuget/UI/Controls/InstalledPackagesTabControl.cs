using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using Rdds.Unity.Nuget.Services;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class InstalledPackagesTabControl : TabControl
  {
    private readonly INugetService _nugetService;
    private readonly InstalledPackagesService _installedPackagesService;
    private readonly ILogger _logger;

    private readonly VisualElement _tabRoot;
    private readonly Label _emptyPlaceholderLabel;
    private readonly VisualElement _packagesContainer;
    
    private CancellationTokenSource? _cancellationTokenSource;

    protected override async void OnSelected()
    {
      base.OnSelected();
      await InitializeAsync();
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private async Task InitializeAsync()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      _packagesContainer.Clear();
      var installedPackages =
        await _installedPackagesService.UpdateInstalledPackagesListAsync(_cancellationTokenSource.Token);

      if (!installedPackages.Any())
      {
        if (!HasChild(_tabRoot, _emptyPlaceholderLabel)) 
          _tabRoot.Add(_emptyPlaceholderLabel);

        return;
      }

      if (HasChild(_tabRoot, _emptyPlaceholderLabel)) 
        _tabRoot.Remove(_emptyPlaceholderLabel);

      foreach (var package in installedPackages)
      {
        if (_cancellationTokenSource.IsCancellationRequested)
          return;
        
        var control = new PackageControl(_packagesContainer, package, _nugetService, _logger);
        await control.InitializeAsync();
      }
    }

    private bool HasChild(VisualElement parent, VisualElement child) => parent.Children().Contains(child);

    public InstalledPackagesTabControl(VisualElement tabRoot, string title, INugetService nugetService,
      InstalledPackagesService installedPackagesService, ILogger logger) : base(tabRoot, title)
    {
      _tabRoot = tabRoot;
      _nugetService = nugetService;
      _logger = logger;
      _installedPackagesService = installedPackagesService;
      _packagesContainer = tabRoot.Q<VisualElement>("InstalledPackagesContainer");
      _emptyPlaceholderLabel = tabRoot.Q<Label>("EmptyPlaceholderLabel");
    }
  }
}
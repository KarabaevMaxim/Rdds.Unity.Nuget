﻿using System.Linq;
using System.Threading;
using NuGet.Common;
using Rdds.Unity.Nuget.Services;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class SearchPackagesTabControl : TabControl
  {
    private readonly INugetService _nugetService;
    private readonly ILogger _logger;

    private readonly VisualElement _packagesContainer;
    private readonly TextField _filterStringTextField;
    private readonly VisualElement _header;
    private readonly PopupField<string> _sourcesControl;
    
    private CancellationTokenSource? _searchCancellationTokenSource;

    private void InitializeSourcesControl(string selected)
    {
      _sourcesControl.value = selected;
      _sourcesControl.AddToClassList("SourcesControl");
      _header.Add(_sourcesControl);
      _sourcesControl.RegisterValueChangedCallback(OnSourcesControlValueChanged);
    }
    
    private async void OnSearchButtonClicked()
    {
      _packagesContainer.Clear();
      _searchCancellationTokenSource?.Cancel();
      _searchCancellationTokenSource = new CancellationTokenSource();
      var packages = 
        await _nugetService.SearchPackagesAsync(_filterStringTextField.text, 0, 20, _searchCancellationTokenSource.Token);

      foreach (var package in packages)
      {
        if (_searchCancellationTokenSource.IsCancellationRequested)
          return;
        
        var control = new PackageControl(_packagesContainer, package, _nugetService, _logger);
        await control.InitializeAsync();
      }
    }

    private void OnSourcesControlValueChanged(ChangeEvent<string> args) => _nugetService.ChangeActiveSource(args.newValue);

    public SearchPackagesTabControl(VisualElement tabRoot, string title, INugetService nugetService,
      NugetConfigService nugetConfigService, ILogger logger) : base(tabRoot, title)
    {
      _nugetService = nugetService;
      _logger = logger;
      _packagesContainer = tabRoot.Q<VisualElement>("PackagesContainer");
      var searchButton = tabRoot.Q<Button>("SearchButton");
      searchButton.clickable.clicked += OnSearchButtonClicked;
      _filterStringTextField = tabRoot.Q<TextField>("FilterStringTextField");
      _header = tabRoot.Q<VisualElement>("Header");
      _sourcesControl = new PopupField<string>(nugetConfigService.GetAvailableSources().ToList(), 0);
      InitializeSourcesControl(_nugetService.SelectedSource.Key);
    }
  }
}
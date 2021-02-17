﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.NewUI.Controls;
using Rdds.Unity.Nuget.NewUI.Controls.Models;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable Unity.NoNullCoalescing

namespace Rdds.Unity.Nuget.NewUI
{
  public class MainWindow : EditorWindow
  {
    private VisualElement _leftPanel = null!;
    private VisualElement _rightPanel = null!;
    private VisualElement _assembliesPopupPlaceholder = null!;
    private VisualElement _sourcesListPlaceholder = null!;
    private TwoPaneSplitView _root = null!;
    private PackageDetailsControl _packageDetailsControl = null!;
    private TextField _filterTextField = null!;

    private PackagesListControl _installedListControl = null!;
    
    private List<PackageRowPresentationModel> _allInstalledPackages = null!;

    private IEnumerable<AssemblyModel> _assemblies = null!;
    
    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
    private CancellationTokenSource? _filterByStringDelayCancellationTokenSource;
    
    [MenuItem("Rdds/New Unity.Nuget")]
    public static void ShowDefaultWindow()
    {
      var wnd = GetWindow<MainWindow>();
      wnd.titleContent = new GUIContent("Nuget package manager");
    }

    private async void OnEnable()
    {
      EditorContext.NugetConfigService.LoadConfigFile();
      await EditorContext.PackagesFileService.LoadPackagesFileAsync();
      
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.MainWindowLayout);
      visualTree.CloneTree(rootVisualElement);
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.Styles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UI.Paths.CommonStyles));

      _root = rootVisualElement.Q<TwoPaneSplitView>("Root");
      _leftPanel = _root.Q<VisualElement>("LeftPanel");
      _rightPanel = _root.Q<VisualElement>("RightPanel");
      _assembliesPopupPlaceholder = _leftPanel.Q<VisualElement>("AssembliesPopupPlaceholder");
      _sourcesListPlaceholder = _leftPanel.Q<VisualElement>("SourcesListPlaceholder");
      _filterTextField = _leftPanel.Q<TextField>("FilterTextField");
      _filterTextField.RegisterValueChangedCallback(OnFilterTextValueChanged);
      
      _root.fixedPaneIndex = 1;
      _root.fixedPaneInitialDimension = 300;

      _assemblies = await EditorContext.AssembliesService.RequireAllAssembliesAsync();
      
      AddAssembliesListPopup();
      AddSourcesListPopup();

      _packageDetailsControl = new PackageDetailsControl(_rightPanel);
      PackagesFiltrationHelper.ShowedInstalledPackages = await RequireInstalledPackages();

      _installedListControl = new PackagesListControl(_leftPanel, "Installed", 200, PackagesFiltrationHelper.ShowedInstalledPackages.ToList(),  OnPackagesListSelectionChangedAsync);
      _ = new PackagesListControl(_leftPanel, "Available", 200, new List<PackageRowPresentationModel>(),  OnPackagesListSelectionChangedAsync);
    }

    private async Task<IEnumerable<PackageRowPresentationModel>> RequireInstalledPackages()
    {
      var installedPackagesService = EditorContext.InstalledPackagesService;
      var packagesFileService = EditorContext.PackagesFileService;
      var packages = installedPackagesService.RequireInstalledPackagesList();

      var tasks = packages.Select(async p =>
      {
        var icon = p.IconPath == null
          ? Resources.Load<Texture>(Paths.DefaultIconResourceName)
          : await ImageHelper.LoadImageAsync(p.IconPath, CancellationToken.None);
        var source = packagesFileService.RequirePackage(p.Identity.Id).Source;

        return new PackageRowPresentationModel(p.Identity.Id, p.Identity.Version.ToString(),
          // todo initialize assemblies in which package installed
          icon ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName), new List<string> {source}, new List<string>());
      });

      var models = await Task.WhenAll(tasks);
      return models;
    }

    private void AddAssembliesListPopup()
    {
      var assembliesNames = new List<string> {"All assemblies"};
      assembliesNames.AddRange(_assemblies.Select(a => a.Name));
      
      var popup = new PopupField<string>(assembliesNames, 0);
      _assembliesPopupPlaceholder.Add(popup);
      popup.RegisterValueChangedCallback(OnAssembliesListPopupValueChanged);
    }

    private void AddSourcesListPopup()
    {
      var sources = new List<string> {"All sources"};
      sources.AddRange(EditorContext.NugetConfigService.RequireAvailableSources());

      var popup = new PopupField<string>(sources, 0);
      _sourcesListPlaceholder.Add(popup);
      popup.RegisterValueChangedCallback(OnSourcesListPopupValueChanged);
    }

    private void OnSourcesListPopupValueChanged(ChangeEvent<string> args)
    {
    }
    
    private void OnAssembliesListPopupValueChanged(ChangeEvent<string> args)
    {
    }

    private async void OnPackagesListSelectionChangedAsync(PackageRowPresentationModel selected)
    {
      _packageDetailsControl.Reset();
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();
      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version)); 
      var installed = EditorContext.InstalledPackagesService.IsPackageInstalled(selected.Id);
      var installIcon = installed
        ? ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName)
        : ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName);
      Action installAction = () => { };
      Action? updateAction = installed && !EditorContext.InstalledPackagesService.EqualInstalledPackageVersion(identity)
                         ? () => { }
                         : (Action?)null;
      
      var assemblies = _assemblies
        .Select(a =>
          new AssemblyPackageDetailsPresentationModel(ImageHelper.LoadBuiltinImage("AssemblyDefinitionAsset Icon"),
            a.Name, null, ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName), () => { }))
        .ToList();
      var details = new PackageDetailsPresentationModel(selected.Id, selected.Icon, selected.Version,
        new List<string> {selected.Version}, selected.Sources.First(), selected.Sources, null,
        null, installIcon, installAction, updateAction, assemblies);
      _packageDetailsControl.Details = details;

      var versions = await EditorContext.NugetService.GetPackageVersionsAsync(selected.Id, _loadDetailsCancellationTokenSource.Token);
      details.Versions = versions.Select(v => v.ToString()).ToList();
      // delayed adding versions
      _packageDetailsControl.Details = details;
      
      var detailInfo = await EditorContext.NugetService.GetPackageAsync(identity, _loadDetailsCancellationTokenSource.Token);

      if (detailInfo == null)
      {
        LogHelper.LogWarning($"Package {identity.Id} with version {identity.Version} not found in online sources");
        return;
      }
      
      details.Dependencies = detailInfo.Dependencies?
      .Select(d => new DependenciesPresentationModel(d.TargetFramework.Name, d.Dependencies
        .Select(dd => new DependencyPresentationModel(dd.Id, dd.Version.ToString()))));
      details.Description = detailInfo.Description;
      
      // delayed adding dependencies and description
      _packageDetailsControl.Details = details;
    }

    private async void OnFilterTextValueChanged(ChangeEvent<string> args)
    {
      _filterByStringDelayCancellationTokenSource?.Cancel();
      _filterByStringDelayCancellationTokenSource?.Dispose();
      _filterByStringDelayCancellationTokenSource = new CancellationTokenSource();

      try
      {
        // wait for 1 sec after last input
         await Task.Delay(1000, _filterByStringDelayCancellationTokenSource.Token).ConfigureAwait(true);
      }
      catch (TaskCanceledException)
      {
        return;
      }
      
      // args.newValue not working with async method
      var filteredInstalledPackages = PackagesFiltrationHelper.FilterByPartId(_filterTextField.text);
      _installedListControl.Refresh(filteredInstalledPackages.ToList());
    }
  }
}
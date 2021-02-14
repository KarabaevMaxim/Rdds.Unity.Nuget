using System;
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

      _root.fixedPaneIndex = 1;
      _root.fixedPaneInitialDimension = 300;
      
      await AddAssembliesListPopupAsync();
      AddSourcesListPopup();

      _packageDetailsControl = new PackageDetailsControl(_rightPanel);
      _ = new PackagesListControl(_leftPanel, "Installed", 200, await RequireInstalledPackages(),  OnPackagesListSelectionChangedAsync);
      _ = new PackagesListControl(_leftPanel, "Available", 200, new List<PackageRowPresentationModel>(),  OnPackagesListSelectionChangedAsync);
    }

    private async Task<List<PackageRowPresentationModel>> RequireInstalledPackages()
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
          icon ?? ImageHelper.LoadImageFromResource(Paths.DefaultIconResourceName), new List<string> {source});
      });

      var models = await Task.WhenAll(tasks);
      return models.ToList();
    }

    private async Task AddAssembliesListPopupAsync()
    {
      var assemblies = await EditorContext.AssembliesService.RequireAllAssembliesAsync();
      var assembliesNames = new List<string> {"All assemblies"};
      assembliesNames.AddRange(assemblies.Select(a => a.Name));
      
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
      var details = new PackageDetailsPresentationModel(selected.Id, selected.Icon, selected.Version,
        new List<string> {selected.Version}, selected.Sources.First(), selected.Sources, null,
        null, installIcon, installAction, updateAction);
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

    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
  }
}
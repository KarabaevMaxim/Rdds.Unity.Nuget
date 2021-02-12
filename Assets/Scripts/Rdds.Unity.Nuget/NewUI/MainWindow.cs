using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.NewUI.Controls;
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
    private VisualElement _assembliesPopupPlaceholder = null!;
    private VisualElement _sourcesListPlaceholder = null!;
    
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
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.PackageRowStyles));

      _leftPanel = rootVisualElement.Q<VisualElement>("LeftPanel");
      _assembliesPopupPlaceholder = _leftPanel.Q<VisualElement>("AssembliesPopupPlaceholder");
      _sourcesListPlaceholder = _leftPanel.Q<VisualElement>("SourcesListPlaceholder");

      AddAssembliesListPopup();
      AddSourcesListPopup();
      
      _ = new PackagesListControl(_leftPanel, "Installed", 200, await RequireInstalledPackages());
      _ = new PackagesListControl(_leftPanel, "Available", 200, new List<PackageRowPresentationModel>());
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
          icon ?? Resources.Load<Texture>(Paths.DefaultIconResourceName), new[] {source});
      });

      var models = await Task.WhenAll(tasks);
      return models.ToList();
    }

    private void AddAssembliesListPopup()
    {
      var assemblies = new List<string> {"All assemblies"};
      assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name));
      
      var popup = new PopupField<string>(assemblies, 0);
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
  }
}
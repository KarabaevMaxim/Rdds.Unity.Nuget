using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.NewUI.Controls;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable Unity.NoNullCoalescing

namespace Rdds.Unity.Nuget.NewUI
{
  public class MainWindow : EditorWindow
  {
    private VisualElement _leftPanel = null!;
    
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
  }
}
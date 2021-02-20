using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.UI;
using Rdds.Unity.Nuget.New.UI.Controls.Models;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class MainWindowPresenter
  {
    private const string AllAssemblies = "All assemblies";
    private const string AllSources = "All sources";

    #region Fields

    private readonly IMainWindow _mainWindow;
    
    private readonly InstalledPackagesPresenter _installedPackagesPresenter;
    private readonly AvailablePackagesPresenter _availablePackagesPresenter;
    private readonly PackageDetailsPresenter _packageDetailsPresenter;

    private CancellationTokenSource? _loadDetailsCancellationTokenSource;
    private CancellationTokenSource? _filterByStringDelayCancellationTokenSource;
    
    #endregion

    private async void Initialize()
    {
      InitializeSources();
      InitializeAssemblies();
      await _installedPackagesPresenter.InitializeAsync();
      _availablePackagesPresenter.Initialize();
    }
    
    private void SelectPackageRow(PackageRowPresentationModel selected)
    {
      _packageDetailsPresenter.Reset();
      _loadDetailsCancellationTokenSource?.Cancel();
      _loadDetailsCancellationTokenSource = new CancellationTokenSource();
      var identity = new PackageIdentity(selected.Id, PackageVersion.Parse(selected.Version)); 
      // var installed = EditorContext.InstalledPackagesService.IsPackageInstalled(selected.Id);
      // var installIcon = installed
      //   ? ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName)
      //   : ImageHelper.LoadImageFromResource(Paths.RemovePackageButtonIconResourceName);
      // Action installAction = () => { };
      // Action? updateAction = installed && !EditorContext.InstalledPackagesService.EqualInstalledPackageVersion(identity)
      //                    ? () => { }
      //                    : (Action?)null;
      //
      // var assemblies = _assemblies
      //   .Select(a =>
      //     new AssemblyPackageDetailsPresentationModel(ImageHelper.LoadBuiltinImage("AssemblyDefinitionAsset Icon"),
      //       a.Name, null, ImageHelper.LoadImageFromResource(Paths.InstallPackageButtonIconResourceName), () => { }))
      //   .ToList();
      // var details = new PackageDetailsPresentationModel(selected.Id, selected.Icon, selected.Version,
      //   new List<string> {selected.Version}, selected.Sources.First(), selected.Sources, null,
      //   null, installIcon, installAction, updateAction, assemblies);
      // _packageDetailsControl.Details = details;
      //
      // var versions = await EditorContext.NugetService.GetPackageVersionsAsync(selected.Id, _loadDetailsCancellationTokenSource.Token);
      // details.Versions = versions.Select(v => v.ToString()).ToList();
      // // delayed adding versions
      // _packageDetailsControl.Details = details;
      //
      // var detailInfo = await EditorContext.NugetService.GetPackageAsync(identity, _loadDetailsCancellationTokenSource.Token);
      //
      // if (detailInfo == null)
      // {
      //   LogHelper.LogWarning($"Package {identity.Id} with version {identity.Version} not found in online sources");
      //   return;
      // }
      //
      // details.Dependencies = detailInfo.Dependencies?
      // .Select(d => new DependenciesPresentationModel(d.TargetFramework.Name, d.Dependencies
      //   .Select(dd => new DependencyPresentationModel(dd.Id, dd.Version.ToString()))));
      // details.Description = detailInfo.Description;
      //
      // // delayed adding dependencies and description
      // _packageDetailsControl.Details = details;
    }

    #region Filtration methods

    private async void FilterByIdAsync(string idPart)
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
      
      //todo check it with ConfigureAwait(true/false)
      // args.newValue not working with async method
      _installedPackagesPresenter.FilterById(idPart);
      _availablePackagesPresenter.FilterById(idPart);
    }
    
    private void FilterByAssembly(string assembly) => 
      _installedPackagesPresenter.FilterByAssembly(assembly == AllAssemblies ? null : assembly);

    private void FilterBySource(string source)
    {
      _installedPackagesPresenter.FilterBySource(source == AllSources ? null : source);
      _availablePackagesPresenter.FilterBySource(source == AllSources ? null : source);
    }

    #endregion

    private void InitializeSources()
    {
      _mainWindow.Sources = new List<string> { AllSources };
    }

    private void InitializeAssemblies()
    {
      _mainWindow.Assemblies = new List<string> { AllAssemblies };
    }
    
    public MainWindowPresenter(IMainWindow mainWindow)
    {
      _mainWindow = mainWindow;
      _installedPackagesPresenter = new InstalledPackagesPresenter(_mainWindow);
      _availablePackagesPresenter = new AvailablePackagesPresenter(_mainWindow);
      _packageDetailsPresenter = new PackageDetailsPresenter(_mainWindow.PackageDetailsControl);
      
      _mainWindow.PackageRowSelected += SelectPackageRow; 
      _mainWindow.FilterTextChanged += FilterByIdAsync;
      _mainWindow.AssemblyChanged += FilterByAssembly;
      _mainWindow.SourceChanged += FilterBySource;
      _mainWindow.WindowPostEnabled += Initialize;
    }
  }
}
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ILogger = NuGet.Common.ILogger;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class OnlinePackageControl : PackageControlBase
  {
    #region Controls
    
    private PopupField<string> _versionsControl = null!;
    
    #endregion

    #region Dependencies

    private readonly NugetService _nugetService;
    private readonly InstalledPackagesService _installedPackagesService;
    private readonly ILogger _logger;

    #endregion

    #region Initialize methods
    
    public async Task InitializeAsync(bool changedVersion)
    {
      if (_installedPackagesService.IsPackageInstalled(PackageInfo.Identity.Id))
      {
        if (_installedPackagesService.EqualInstalledPackageVersion(PackageInfo.Identity))
          ToInstalledState();
        else
          ToInstalledAnotherVersionState();
      }
      else
        ToDefaultState();

      await CreateVersionsControlAsync(!changedVersion);
      await AddDependenciesAsync();
      await SetIconAsync(CancellationToken.None);
    }

    private async Task AddDependenciesAsync()
    {
      var packageInfo = await _nugetService.GetPackageAsync(PackageInfo.Identity, CancellationToken.None);

      if (packageInfo == null)
      {
        SetDependencies();
        return;
      }
      
      PackageInfo = packageInfo;
      SetDependencies();
    }
    
    private async Task CreateVersionsControlAsync(bool takeVersionFromInstalled)
    {
      var versions = 
        await _nugetService.GetPackageVersionsAsync(PackageInfo.Identity.Id, CancellationToken.None);
      
      // ReSharper disable once ConstantConditionalAccessQualifier
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = new PopupField<string>(versions.Select(v => v.ToString()).ToList(), 0);

      _versionsControl.value = _installedPackagesService.IsPackageInstalled(PackageInfo.Identity.Id) && takeVersionFromInstalled
        ? _installedPackagesService.RequireVersionInstalledPackage(PackageInfo.Identity.Id).ToString()
        : PackageInfo.Identity.Version.ToString();
      
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      VersionPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionControlValueChangedAsync);
    }

    #endregion

    #region Methods for changing states

    private void ToDefaultState()
    {
      ActionButtonText = "Download";
      ActionButton.clickable.clicked += OnDownloadButtonClickedAsync;
    }
    
    private void ToDownloadedState(string packageDirectoryPath)
    {
      ActionButtonText = "Install";
      ActionButton.clickable.clicked += async () => await OnInstallButtonClickedAsync(packageDirectoryPath);
    }

    private void ToInstalledAnotherVersionState()
    {
      ActionButtonText = "Update";
      ActionButton.clickable.clicked += OnUpdateButtonClicked;
    }
    
    private void ToInstalledState()
    {
      ActionButtonText = "Remove";
      ActionButton.clickable.clicked += OnRemoveButtonClickedAsync;
    }

    #endregion

    #region Controls' event handlers

    private async void OnDownloadButtonClickedAsync()
    {
      var packageDirectoryPath = await DownloadPackageAsync();
      
      if (packageDirectoryPath == null)
        return;

      ToDownloadedState(packageDirectoryPath);
    }
    
    private async Task OnInstallButtonClickedAsync(string packageDirectoryPath)
    {
      var result = await InstallPackageAsync(packageDirectoryPath);
      
      if (result)
        ToInstalledState();
      else
        DialogHelper.ShowErrorAlert($"Failed to install {PackageInfo.Identity.Id} package");
    }

    private async void OnRemoveButtonClickedAsync()
    {
      var result = await _installedPackagesService.RemovePackageAsync(PackageInfo.Identity, null);

      if (result)
        ToDefaultState();
      else
        DialogHelper.ShowErrorAlert($"Failed to remove {PackageInfo.Identity.Id} package");
    }

    private async void OnUpdateButtonClicked()
    {
      var packageDirectoryPath = await DownloadPackageAsync();
      
      if (packageDirectoryPath == null)
        return;

      var installResult = await InstallPackageAsync(packageDirectoryPath);
      
      if (installResult)
        ToInstalledState();
      else
        DialogHelper.ShowErrorAlert($"Failed to install {PackageInfo.Identity.Id} package");
    }
    
    private async void OnVersionControlValueChangedAsync(ChangeEvent<string> args)
    {
      if (!PackageVersion.TryParse(_versionsControl.value, out var version))
      {
        _versionsControl.value = args.previousValue;
        _logger.LogWarning($"Cannot parse version of package {_versionsControl.value}");
        return;
      }

      if (!await TryChangePackageVersionAsync(version!)) 
        _versionsControl.value = args.previousValue;
    }

    #endregion

    #region Helper methods

    private Task<string?> DownloadPackageAsync()
    {
      var task = _nugetService.DownloadPackageAsync(PackageInfo.Identity, CancellationToken.None);
      return DialogHelper.ShowLoadingAsync("Downloading...", "Wait while package downloading", task);
    }

    private Task<bool> InstallPackageAsync(string packageDirectoryPath)
    {
      var task = _installedPackagesService.InstallPackageAsync(packageDirectoryPath, null, null);
      return DialogHelper.ShowLoadingAsync("Installing...", "Wait while package installing", task);
    }
    
    private async Task<bool> TryChangePackageVersionAsync(PackageVersion newVersion)
    {
      var info = await _nugetService.GetPackageAsync(new PackageIdentity(PackageInfo.Identity.Id, newVersion!), CancellationToken.None);

      if (info == null)
      {
        _logger.LogWarning($"Package {PackageInfo.Identity.Id} with version {newVersion} not found");
        return false;
      }

      PackageInfo = info;
      await InitializeAsync(true);
      return true;
    }

    #endregion

    #region Constructors

    public OnlinePackageControl(VisualElement parent, PackageInfo packageInfo, NugetService nugetService,
      InstalledPackagesService installedPackagesService, ILogger logger) : base(parent, packageInfo)
    {
      _nugetService = nugetService;
      _installedPackagesService = installedPackagesService;
      _logger = logger;
    }

    #endregion
  }
}
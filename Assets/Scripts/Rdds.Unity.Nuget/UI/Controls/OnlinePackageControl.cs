using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using UnityEditor.UIElements;
using UnityEngine;
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

    private readonly INugetService _nugetService;
    private readonly ILogger _logger;

    #endregion

    #region Initialize methods

    public async Task InitializeAsync()
    {
      Title = PackageInfo.Title ?? PackageInfo.Identity.Id;
      Description = PackageInfo.Description ?? "No description";
      ToDefaultState();

      await CreateVersionsControlAsync();
      await AddDependenciesAsync();
      await SetIconAsync();
    }
    
    private async Task SetIconAsync()
    {
      if (PackageInfo.IconPath == null)
        return;
      
      var icon = await ImageHelper.DownloadImageAsync(PackageInfo.IconPath, CancellationToken.None);
      
      if (icon == null)
        icon = Resources.Load<Texture2D>("NugetIcon");

      IconImage.image = icon;
    }
    
    private async Task AddDependenciesAsync()
    {
      var packageInfo = await _nugetService.GetPackageAsync(PackageInfo.Identity, CancellationToken.None);

      if (packageInfo?.Dependencies == null || !packageInfo.Dependencies.Any())
      {
        Dependencies = "No dependencies found";
        return;
      }

      PackageInfo = packageInfo;
      var textDependencies = new StringBuilder();

      foreach (var group in PackageInfo.Dependencies)
      {
        textDependencies.AppendLine(group.TargetFramework.Name);
      
        foreach (var dependency in group.Dependencies)
          textDependencies.AppendLine($"  -{dependency.Id} >= {dependency.Version}");
      }

      Dependencies = textDependencies.ToString();
    }
    
    private async Task CreateVersionsControlAsync()
    {
      var versions = 
        await _nugetService.GetPackageVersionsAsync(PackageInfo.Identity.Id, CancellationToken.None);
      
      // ReSharper disable once ConstantConditionalAccessQualifier
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = new PopupField<string>(versions.Select(v => v.ToString()).ToList(), 0);
      _versionsControl.value = PackageInfo.Identity.Version.ToString();
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      VersionPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionControlValueChanged);
    }

    #endregion

    #region Methods for changing states

    private void ToDefaultState()
    {
      ActionButtonText = "Download";
      ActionButton.clickable.clicked += OnDownloadButtonClicked;
    }
    
    private void ToDownloadedState(string packageDirectoryPath)
    {
      ActionButtonText = "Install";
      ActionButton.clickable.clicked += async () => await OnInstallButtonClickedAsync(packageDirectoryPath);
    }

    private void ToInstalledState()
    {
      ActionButtonText = "Remove";
      ActionButton.clickable.clicked += OnRemoveButtonClicked;
    }

    #endregion

    #region Controls' event handlers

    private async void OnDownloadButtonClicked()
    {
      var packageDirectoryPath = await _nugetService.DownloadPackageAsync(PackageInfo.Identity, CancellationToken.None);

      if (packageDirectoryPath == null)
        return;
      
      ToDownloadedState(packageDirectoryPath);
    }
    
    private async Task OnInstallButtonClickedAsync(string packageDirectoryPath)
    {
      var result = await _nugetService.InstallPackageAsync(packageDirectoryPath);

      if (result)
        ToInstalledState();
      else
      {
        // todo show alert
      }
    }

    private void OnRemoveButtonClicked() => throw new NotImplementedException();

    private async void OnVersionControlValueChanged(ChangeEvent<string> args)
    {
      if (!TryParseVersion(out var version))
      {
        _versionsControl.value = args.previousValue;
        return;
      }

      if (!await TryChangePackageVersionAsync(version!)) 
        _versionsControl.value = args.previousValue;
    }

    #endregion

    #region Helper methods

    private async Task<bool> TryChangePackageVersionAsync(PackageVersion newVersion)
    {
      var info = await _nugetService.GetPackageAsync(new PackageIdentity(PackageInfo.Identity.Id, newVersion!), CancellationToken.None);

      if (info == null)
      {
        _logger.LogWarning($"Package {PackageInfo.Identity.Id} with version {newVersion} not found");
        return false;
      }

      PackageInfo = info;
      await InitializeAsync();
      return true;
    }
    
    private bool TryParseVersion(out PackageVersion? version)
    {
      version = null;
      
      try
      {
        version = PackageVersion.Parse(_versionsControl.value);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"{ex.GetType().Name}:{ex.Message} {ex.StackTrace}");
        return false;
      }
    }

    #endregion

    #region Constructors

    public OnlinePackageControl(VisualElement parent, PackageInfo packageInfo, INugetService nugetService, ILogger logger) : base(parent, packageInfo)
    {
      _nugetService = nugetService;
      _logger = logger;
    }
    
    #endregion
  }
}
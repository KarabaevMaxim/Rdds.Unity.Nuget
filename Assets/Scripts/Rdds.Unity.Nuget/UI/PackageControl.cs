using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = NuGet.Common.ILogger;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI
{
  public class PackageControl
  {
    #region Controls

    private readonly VisualElement _versionPlaceholder;
    private readonly Label _titleLbl;
    private readonly Image _iconImage;
    private readonly Label _descriptionLbl;
    private readonly Button _actionButton;
    private readonly Label _dependenciesLabel;
    private PopupField<string> _versionsControl = null!;
    
    #endregion

    #region Dependencies

    private readonly INugetService _nugetService;
    private readonly ILogger _logger;

    #endregion

    #region Fields and properties

    private PackageInfo _packageInfo;
    
    private string ActionButtonText
    {
      get => _actionButton.text;
      set => _actionButton.text = value;
    }

    #endregion

    #region Initialize methods

    public async Task InitializeAsync()
    {
      _titleLbl.text = _packageInfo.Title;
      _descriptionLbl.text = _packageInfo.Description;
      ToDefaultState();

      await CreateVersionsControlAsync();
      await AddDependenciesAsync();
      await SetIconAsync();
    }
    
    private async Task SetIconAsync()
    {
      var icon = await DownloadHelper.DownloadImageAsync(_packageInfo.IconUrl, CancellationToken.None);
      
      if (icon == null)
        icon = Resources.Load<Texture2D>("NugetIcon");

      _iconImage.image = icon;
    }
    
    private async Task AddDependenciesAsync()
    {
      var packageInfo = await _nugetService.GetPackageAsync(_packageInfo.Identity, CancellationToken.None);

      if (packageInfo?.Dependencies == null || !packageInfo.Dependencies.Any())
      {
        _dependenciesLabel.text = "No dependencies found";
        return;
      }

      _packageInfo = packageInfo;
      var textDependencies = new StringBuilder();

      foreach (var group in _packageInfo.Dependencies)
      {
        textDependencies.AppendLine(group.TargetFramework.Name);
      
        foreach (var dependency in group.Dependencies)
          textDependencies.AppendLine($"  -{dependency.Id} >= {dependency.Version}");
      }

      _dependenciesLabel.text = textDependencies.ToString();
    }
    
    private async Task CreateVersionsControlAsync()
    {
      var versions = 
        await _nugetService.GetPackageVersionsAsync(_packageInfo.Identity.Id, CancellationToken.None);
      
      // ReSharper disable once ConstantConditionalAccessQualifier
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = new PopupField<string>(versions.Select(v => v.ToString()).ToList(), 0);
      _versionsControl.value = _packageInfo.Identity.Version.ToString();
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      _versionPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionControlValueChanged);
    }

    #endregion

    #region Methods for changing states

    private void ToDefaultState()
    {
      ActionButtonText = "Download";
      _actionButton.clickable.clicked += OnDownloadButtonClicked;
    }
    
    private void ToDownloadedState()
    {
      ActionButtonText = "Install";
      _actionButton.clickable.clicked += OnInstallButtonClicked;
    }

    private void ToInstalledState()
    {
      ActionButtonText = "Remove";
    }

    #endregion

    #region Controls' event handlers

    private async void OnDownloadButtonClicked()
    {
      var downloadResult = await _nugetService.DownloadPackageAsync(_packageInfo.Identity, CancellationToken.None);

      if (!downloadResult)
        return;
      
      ToDownloadedState();
    }
    
    private void OnInstallButtonClicked() => throw new NotImplementedException();

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
      var info = await _nugetService.GetPackageAsync(new PackageIdentity(_packageInfo.Identity.Id, newVersion!), CancellationToken.None);

      if (info == null)
      {
        _logger.LogWarning($"Package {_packageInfo.Identity.Id} with version {newVersion} not found");
        return false;
      }

      _packageInfo = info;
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

    public PackageControl(VisualElement parent, PackageInfo info, INugetService nugetService, ILogger logger)
    {
      _packageInfo = info;
      _nugetService = nugetService;
      _logger = logger;
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/PackageControl.uxml");
      var root = visualTree.CloneTree();
      parent.Add(root);
      _versionPlaceholder = root.Q<VisualElement>("VersionPlaceholder");
      _titleLbl = root.Q<Label>("TitleLbl");
      _descriptionLbl = root.Q<Label>("DescriptionLbl");
      _iconImage = root.Q<Image>("IconImage");
      _actionButton = root.Q<Button>("ActionButton");
      _dependenciesLabel = root.Q<Label>("DependenciesLabel");
    }

    #endregion
  }
}
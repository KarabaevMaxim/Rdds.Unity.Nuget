using System;
using System.Collections.Generic;
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
    private readonly VisualElement _versionPlaceholder;
    private readonly Label _titleLbl;
    private readonly Image _iconImage;
    private readonly Label _descriptionLbl;
    private readonly Button _actionButton;
    private readonly Foldout _dependenciesPanel;
    private readonly Label _dependenciesLabel;
    private PopupField<string> _versionsControl = null!;
    
    private PackageInfo _packageInfo;
    private readonly INugetService _nugetService;
    private readonly ILogger _logger;

    private string ActionButtonText
    {
      get => _actionButton.text;
      set => _actionButton.text = value;
    }

    private PackageInfo PackageInfo
    {
      set
      {
        _packageInfo = value;
        
      }
      get => _packageInfo;
    }
    
    public async Task UpdateFields()
    {
      _titleLbl.text = _packageInfo.Title;
      _descriptionLbl.text = _packageInfo.Description;
      AddDependencies();
      var versions = 
        await _nugetService.GetPackageVersionsAsync(_packageInfo.Identity.Id, CancellationToken.None);
      CreateVersionsControl(_packageInfo.Identity.Version, versions);
      
      ToDefaultState();

      await SetIconAsync();
    }

    private void AddDependencies()
    {
      if (!_packageInfo.Dependencies.Any())
      {
        _dependenciesPanel.value = false;
        _dependenciesLabel.text = "No dependencies found";
        return;
      }

      var textDependencies = new StringBuilder();
      
      foreach (var group in _packageInfo.Dependencies)
      {
        textDependencies.AppendLine(group.TargetFramework);

        foreach (var dependency in group.Dependencies)
          textDependencies.AppendLine($"  {dependency.Id} >= {dependency.Version}");
      }

      _dependenciesLabel.text = textDependencies.ToString();
      _dependenciesPanel.value = true;
    }

    private void CreateVersionsControl(PackageVersion selectedVersion, IEnumerable<PackageVersion> availableVersions)
    {
      // ReSharper disable once ConstantConditionalAccessQualifier
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = new PopupField<string>(availableVersions.Select(v => v.ToString()).ToList(), 0);
      _versionsControl.value = selectedVersion.ToString();
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      _versionPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionControlValueChanged);
    }

    private async Task SetIconAsync()
    {
      var icon = await DownloadHelper.DownloadImageAsync(_packageInfo.IconUrl, CancellationToken.None);
      
      if (icon == null)
        icon = Resources.Load<Texture2D>("NugetIcon");

      _iconImage.image = icon;
    }

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
      PackageVersion version = null!;
      
      try
      {
        version = PackageVersion.Parse(args.newValue);
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"{ex.GetType().Name}:{ex.Message} {ex.StackTrace}");
        _versionsControl.value = args.previousValue;
        return;
      }
      
      var info = await _nugetService.GetPackageAsync(_packageInfo.Identity.Id, version, CancellationToken.None);

      if (info == null)
      {
        _logger.LogWarning($"Package {_packageInfo.Identity.Id} with version {version} not found");
        _versionsControl.value = args.previousValue;
        return;
      }

      _packageInfo = info;
      await UpdateFields();
    }

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
      _dependenciesPanel = root.Q<Foldout>("DependenciesPanel");
      _dependenciesLabel = _dependenciesPanel.Q<Label>("DependenciesLabel"); }
  }
}
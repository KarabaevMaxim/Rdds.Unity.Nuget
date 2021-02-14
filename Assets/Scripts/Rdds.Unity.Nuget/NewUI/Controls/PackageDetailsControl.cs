using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Rdds.Unity.Nuget.NewUI.Controls.Models;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.NewUI.Controls
{
  internal class PackageDetailsControl
  {
    private static readonly VisualTreeAsset _treeAsset;
    private readonly Image _headerIcon;
    private readonly Label _headerId;
    private readonly VisualElement _versionsPlaceholder;
    private readonly VisualElement _sourcesPlaceholder;
    private readonly Button _installRemoveInAllAssembliesButton;
    private readonly Button _updateInAllAssembliesButton;
    private readonly Image _installRemoveInAllAssembliesButtonIcon;
    private readonly VisualElement _subHeader;
    private readonly Label _descriptionLabel;
    private readonly Label _dependenciesLabel;
    
    private PopupField<string>? _versionsControl;
    private PopupField<string>? _sourcesControl;

    private PackageDetailsPresentationModel _details;


    public PackageDetailsPresentationModel? Details
    {
      private get => _details;
      set
      {
        if (!value.HasValue)
          return;
        
        _details = value.Value;
        Id = _details.Id;
        _headerIcon.image = _details.Icon;
        CreateVersionsControl(_details.SelectedVersion, _details.Versions);
        CreateSourcesControl(_details.SelectedSource, _details.AvailableInSources);
        _installRemoveInAllAssembliesButtonIcon.image = _details.InstallRemoveButtonIcon;
        UpdateButtonVisible = _details.UpdateButtonAction != null;
        CreateDependencies(_details.Dependencies);
      }
    }
    
    private string Id
    {
      get => _headerId.text;
      set => _headerId.text = value;
    }

    private bool UpdateButtonVisible
    {
      get => _updateInAllAssembliesButton.visible;
      set
      {
        _updateInAllAssembliesButton.visible = value;
        
        if (_updateInAllAssembliesButton.visible)
          _subHeader.Insert(2, _updateInAllAssembliesButton);
        else if (_updateInAllAssembliesButton.parent != null) 
          _subHeader.Remove(_updateInAllAssembliesButton);
      }
    }

    private string? Dependencies
    {
      get => _dependenciesLabel.text;
      set => _dependenciesLabel.text = string.IsNullOrWhiteSpace(value) ? "No dependencies" : value;
    }

    private string? Description
    {
      get => _descriptionLabel.text;
      set => _descriptionLabel.text = string.IsNullOrWhiteSpace(value) ? "No description" : value;
    }

    public void Reset()
    {
      UpdateButtonVisible = false;
      _sourcesControl?.RemoveFromHierarchy();
      _versionsControl?.RemoveFromHierarchy();
      Dependencies = null;
    }
    
    private void CreateVersionsControl(string selectedVersion, List<string> versions)
    {
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = new PopupField<string>(versions, 0);
      _versionsControl.value = selectedVersion;
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      _versionsPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionsControlValueChanged);
    }

    private void CreateSourcesControl(string selectedSource, List<string> sources)
    {
      _sourcesControl?.RemoveFromHierarchy();
      _sourcesControl = new PopupField<string>(sources, 0);
      _sourcesControl.value = selectedSource;
      _sourcesControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      _sourcesPlaceholder.Add(_sourcesControl);
      _sourcesControl.RegisterValueChangedCallback(OnSourcesControlValueChanged);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private void CreateDependencies(IEnumerable<DependenciesPresentationModel>? dependencies)
    {
      if (dependencies == null || !dependencies.Any())
      {
        Dependencies = null;
        return;
      }
      
      var sb = new StringBuilder();

      foreach (var dependencyGroup in dependencies)
      {
        sb.AppendLine($"{dependencyGroup.TargetFramework}:");

        foreach (var dependency in dependencyGroup.Dependencies)
          sb.AppendLine($"  -{dependency.Name} >= {dependency.MinVersion}");
      }

      Dependencies = sb.ToString();
    }
    
    private void OnSourcesControlValueChanged(ChangeEvent<string> evt)
    {
    }

    private void OnVersionsControlValueChanged(ChangeEvent<string> evt)
    {
    }

    private void OnInstallRemoveInAllAssembliesButtonClicked() => Details!.Value.InstallRemoveButtonAction?.Invoke();

    private void OnUpdateInAllAssembliesButtonClicked() => Details!.Value.UpdateButtonAction?.Invoke();

    public PackageDetailsControl(VisualElement parent)
    {
      var root = _treeAsset.CloneTree();
      root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.PackageDetailsStyles));
      parent.Add(root);

      var header = root.Q<VisualElement>("Header");
      _headerIcon = header.Q<Image>("Icon");
      _headerId = header.Q<Label>("Id");
      _subHeader = root.Q<VisualElement>("SubHeader");
      _versionsPlaceholder = _subHeader.Q<VisualElement>("VersionsPlaceholder");
      _sourcesPlaceholder = _subHeader.Q<VisualElement>("SourcesPlaceholder");
      _installRemoveInAllAssembliesButton = _subHeader.Q<Button>("InstallRemoveInAllAssembliesButton");
      _installRemoveInAllAssembliesButtonIcon = _installRemoveInAllAssembliesButton.Q<Image>("ButtonIcon");
      _installRemoveInAllAssembliesButton.clickable.clicked += OnInstallRemoveInAllAssembliesButtonClicked;
      _updateInAllAssembliesButton = _subHeader.Q<Button>("UpdateInAllAssembliesButton");
      _updateInAllAssembliesButton.clickable.clicked += OnUpdateInAllAssembliesButtonClicked;
      _updateInAllAssembliesButton.Q<Image>("ButtonIcon").image = ImageHelper.LoadImageFromResource(Paths.UpdatePackageButtonIconResourceName);
      _descriptionLabel = root.Q<Label>("DescriptionLabel");
      _dependenciesLabel = root.Q<Label>("DependenciesLabel");
      
      Reset();
    }
    
    static PackageDetailsControl() => _treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackageDetailControlLayout);
  }
}
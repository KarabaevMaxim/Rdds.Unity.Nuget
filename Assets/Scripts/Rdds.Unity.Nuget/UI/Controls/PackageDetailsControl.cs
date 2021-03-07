using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Rdds.Unity.Nuget.UI.Controls.Models;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class PackageDetailsControl
  {
    #region Fields and properties

    private static readonly VisualTreeAsset _mainTreeAsset;
    private static readonly VisualTreeAsset _assemblyRowTreeAsset;
    private static readonly StyleSheet _styles;
    
    private readonly Image _headerIcon;
    private readonly Label _headerId;
    private readonly VisualElement _versionsPlaceholder;
    private readonly VisualElement _sourcesPlaceholder;
    private readonly Button _updateInAllAssembliesButton;
    private readonly Image _installRemoveInAllAssembliesButtonIcon;
    private readonly VisualElement _subHeader;
    private readonly Label _descriptionLabel;
    private readonly Label _dependenciesLabel;
    private readonly VisualElement _assembliesPanel;
    private readonly VisualElement _contentPanel;
    private readonly Label _assembliesNotFoundLabel;
    private readonly ListView _assembliesListView;
    private readonly Button _installRemoveInAllAssembliesButton;

    private PopupField<string>? _versionsControl;
    private PopupField<string>? _sourcesControl;

    private PackageDetailsPresentationModel _details;
    private bool _isLoading;

    public PackageDetailsPresentationModel? Details
    {
      private get => _details;
      set
      {
        if (!value.HasValue)
          return;

        Reset();
        _contentPanel.visible = true;
        _details = value.Value;
        Id = _details.Id;
        _headerIcon.image = _details.Icon;
        Description = _details.Description;
        _installRemoveInAllAssembliesButtonIcon.image = _details.InstallRemoveButtonIcon;
        UpdateButtonVisible = _details.UpdateButtonAction != null;
        CreateVersionsControl(_details.SelectedVersion, _details.Versions);
        CreateSourcesControl(_details.SelectedSource, _details.AvailableInSources);
        CreateDependencies(_details.Dependencies);
        CreateAssembliesList(_details.Assemblies);
      }
    }

    public string? SelectedSource => _sourcesControl?.value;
    
    public string? SelectedVersion => _versionsControl?.value;

    public bool IsLoading
    {
      get => _isLoading;
      set
      {
        if (_isLoading == value)
          return;

        _isLoading = value;

        if (_isLoading)
          DisableControls();
        else
          EnableControls();
      }
      
    }
    
    private string Id
    {
      set => _headerId.text = value;
    }

    private bool UpdateButtonVisible
    {
      set
      {
        _updateInAllAssembliesButton.visible = value;
        
        if (_updateInAllAssembliesButton.visible)
          _subHeader.Insert(2, _updateInAllAssembliesButton);
        else if (_updateInAllAssembliesButton.parent != null) 
          _subHeader.Remove(_updateInAllAssembliesButton);
      }
    }

    private bool AssembliesNotFound
    {
      set
      {
        if (value)
        {
          _assembliesListView.RemoveFromHierarchy();
          _assembliesPanel.Add(_assembliesNotFoundLabel);
        }
        else
        {
          _assembliesNotFoundLabel.RemoveFromHierarchy();
          _assembliesPanel.Add(_assembliesListView);
        }
      }
    }
    
    private string? Dependencies
    {
      set => _dependenciesLabel.text = string.IsNullOrWhiteSpace(value) ? "No dependencies" : value;
    }

    private string? Description
    {
      set => _descriptionLabel.text = string.IsNullOrWhiteSpace(value) ? "No description" : value;
    }

    #endregion

    private void Reset()
    {
      _contentPanel.visible = false;
      UpdateButtonVisible = false;
      _sourcesControl?.RemoveFromHierarchy();
      _sourcesControl = null;
      _versionsControl?.RemoveFromHierarchy();
      _versionsControl = null;
      Dependencies = null;
      Description = null;
    }
    
    private void CreateVersionsControl(string selectedVersion, IEnumerable<string> versions)
    {
      _versionsControl?.RemoveFromHierarchy();
      var items = versions.ToList();

      if (items.Count == 0)
        return;

      _versionsControl = new PopupField<string>(items, 0);
      _versionsControl.value = selectedVersion;
      _versionsControl.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      _versionsPlaceholder.Add(_versionsControl);
      _versionsControl.RegisterValueChangedCallback(OnVersionsControlValueChanged);
    }

    private void CreateSourcesControl(string selectedSource, IEnumerable<string> sources)
    {
      _sourcesControl?.RemoveFromHierarchy();
      
      var items = sources.ToList();

      if (items.Count == 0)
        return;
      
      _sourcesControl = new PopupField<string>(items, 0);
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

    private void CreateAssembliesList(IEnumerable<AssemblyPackageDetailsPresentationModel> assemblies)
    {
      var assembliesList = assemblies.ToList();
      
      if (assembliesList.Count == 0)
      {
        AssembliesNotFound = true;
        return;
      }

      AssembliesNotFound = false;
      _assembliesListView.itemHeight = 40;
      _assembliesListView.style.height = new StyleLength(new Length(200));
      _assembliesListView.selectionType = SelectionType.None;
      _assembliesListView.itemsSource = assembliesList;
      
      _assembliesListView.bindItem -= OnAssembliesListViewBindItem;
      _assembliesListView.bindItem += OnAssembliesListViewBindItem;
    }

    private void DisableControls()
    {
      _versionsControl?.SetEnabled(false);
      _sourcesControl?.SetEnabled(false);
      _installRemoveInAllAssembliesButton.SetEnabled(false);
      _updateInAllAssembliesButton.SetEnabled(false);
      FindAssembliesButtons().ForEach(b => b.SetEnabled(false));
    }

    private void EnableControls()
    {
      _versionsControl?.SetEnabled(true);
      _sourcesControl?.SetEnabled(true);
      _installRemoveInAllAssembliesButton.SetEnabled(true);
      _updateInAllAssembliesButton.SetEnabled(true);
      FindAssembliesButtons().ForEach(b => b.SetEnabled(true));
    }

    private List<Button> FindAssembliesButtons() => _assembliesListView.Query<Button>("ActionButton").Build().ToList();

    private void OnSourcesControlValueChanged(ChangeEvent<string> evt)
    {
    }

    private void OnVersionsControlValueChanged(ChangeEvent<string> evt)
    {
    }

    private void OnInstallRemoveInAllAssembliesButtonClicked() => Details!.Value.InstallRemoveButtonAction?.Invoke();

    private void OnUpdateInAllAssembliesButtonClicked() => Details!.Value.UpdateButtonAction?.Invoke();
    
    private void OnAssembliesListViewBindItem(VisualElement row, int index)
    {
      var model = (AssemblyPackageDetailsPresentationModel)_assembliesListView.itemsSource[index];
      
      row.Q<Image>("Icon").image = model.Icon;
      row.Q<Label>("Name").text = model.Name;
      row.Q<Label>("InstalledVersionOfPackage").text = model.InstalledVersionOfPackage ?? string.Empty;
      row.Q<Button>("ActionButton").clickable.clicked += model.ButtonAction;
      row.Q<Image>("ButtonIcon").image = model.ButtonIcon;
    }

    #region Constructors

    public PackageDetailsControl(VisualElement parent)
    {
      var root = _mainTreeAsset.CloneTree();
      root.styleSheets.Add(_styles);
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
      _contentPanel = root.Q<VisualElement>("ContentPanel");
      _assembliesPanel = _contentPanel.Q<Foldout>("AssembliesPanel");
      _assembliesNotFoundLabel = _assembliesPanel.Q<Label>("AssembliesNotFoundLabel");
      _assembliesListView = _assembliesPanel.Q<ListView>("ListView");
      _assembliesListView.makeItem += _assemblyRowTreeAsset.CloneTree;

      Reset();
    }
    
    static PackageDetailsControl()
    {
      _mainTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackageDetailControlLayout);
      _assemblyRowTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.AssemblyRowLayout);
      _styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.PackageDetailsStyles);
    }

    #endregion
  }
}
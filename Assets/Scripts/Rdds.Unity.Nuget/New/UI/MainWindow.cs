using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.New.UI.Controls;
using Rdds.Unity.Nuget.New.UI.Controls.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable Unity.NoNullCoalescing

namespace Rdds.Unity.Nuget.New.UI
{
  internal class MainWindow : EditorWindow, IMainWindow
  {
    #region Fields

    private List<PackageRowPresentationModel> _installedPackages = null!;
    private List<PackageRowPresentationModel> _availablePackages = null!;
    private List<string> _sources = null!;
    private List<string> _assemblies = null!;
    
    private VisualElement _leftPanel = null!;
    private VisualElement _rightPanel = null!;
    private VisualElement _assembliesPopupPlaceholder = null!;
    private VisualElement _sourcesListPlaceholder = null!;
    private TwoPaneSplitView _root = null!;
    private TextField _filterTextField = null!;

    private PackagesListControl? _installedListControl;
    private PackagesListControl? _availableListControl;

    #endregion

    #region Properties

    public List<PackageRowPresentationModel> InstalledPackages
    {
      get => _installedPackages;
      set
      {
        _installedPackages = value;

        if (_installedListControl == null)
          _installedListControl = new PackagesListControl(_leftPanel, "Installed", 200, _installedPackages,
            OnPackagesListSelectionChanged);
        else
          _installedListControl.Refresh(_installedPackages);
      }
    }
    
    public List<PackageRowPresentationModel> AvailablePackages
    {
      get => _availablePackages;
      set
      {
        _availablePackages = value;

        if (_availableListControl == null)
          _availableListControl = new PackagesListControl(_leftPanel, "Available", 200, AvailablePackages,
            OnPackagesListSelectionChanged);
        else
          _availableListControl.Refresh(_availablePackages);
      }
    }
    
    public List<string> Sources
    {
      get => _sources;
      set
      {
        _sources = value;
        AddSourcesListPopup(_sources);
      }
    }
    
    public List<string> Assemblies
    {
      get => _assemblies;
      set
      {
        _assemblies = value;
        AddAssembliesListPopup(_assemblies);
      }
    }

    public PackageDetailsControl PackageDetailsControl { get; private set; } = null!;

    #endregion

    #region Events

    public event Action<PackageRowPresentationModel>? PackageRowSelected;
    public event Action<string>? FilterTextChanged;
    public event Action<string>? AssemblyChanged;
    public event Action<string>? SourceChanged;
    public event Action? WindowPostEnabled;

    #endregion

    #region Unity methods

    private void OnEnable()
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.MainWindowLayout);
      visualTree.CloneTree(rootVisualElement);
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Paths.Styles));
      rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Nuget.UI.Paths.CommonStyles));

      _root = rootVisualElement.Q<TwoPaneSplitView>("Root");
      _leftPanel = _root.Q<VisualElement>("LeftPanel");
      _rightPanel = _root.Q<VisualElement>("RightPanel");
      _assembliesPopupPlaceholder = _leftPanel.Q<VisualElement>("AssembliesPopupPlaceholder");
      _sourcesListPlaceholder = _leftPanel.Q<VisualElement>("SourcesListPlaceholder");
      _filterTextField = _leftPanel.Q<TextField>("FilterTextField");
      _filterTextField.RegisterValueChangedCallback(OnFilterTextValueChanged);
      
      _root.fixedPaneIndex = 1;
      _root.fixedPaneInitialDimension = 300;

      PackageDetailsControl = new PackageDetailsControl(_rightPanel);
      WindowPostEnabled?.Invoke();
    }

    #endregion

    #region Controls creating methods

    private void AddAssembliesListPopup(List<string> items)
    {
      var popup = new PopupField<string>(items, 0);
      _assembliesPopupPlaceholder.Add(popup);
      popup.RegisterValueChangedCallback(OnAssembliesListPopupValueChanged);
    }

    private void AddSourcesListPopup(List<string> items)
    {
      var popup = new PopupField<string>(items, 0);
      _sourcesListPlaceholder.Add(popup);
      popup.RegisterValueChangedCallback(OnSourcesListPopupValueChanged);
    }

    #endregion
    
    #region Controls event handlers

    private void OnSourcesListPopupValueChanged(ChangeEvent<string> args) => SourceChanged?.Invoke(args.newValue);

    private void OnAssembliesListPopupValueChanged(ChangeEvent<string> args) => AssemblyChanged?.Invoke(args.newValue);

    private void OnPackagesListSelectionChanged(PackageRowPresentationModel selected) => 
      PackageRowSelected?.Invoke(selected);

    private void OnFilterTextValueChanged(ChangeEvent<string> args) => FilterTextChanged?.Invoke(args.newValue);

    #endregion
  }
}
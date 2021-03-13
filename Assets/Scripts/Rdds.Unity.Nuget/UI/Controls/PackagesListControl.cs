using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rdds.Unity.Nuget.UI.Controls.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class PackagesListControl
  {
    private const int ItemHeight = 30;
    
    private readonly Label _titleLabel;
    private readonly ListView _listView;
    
    private readonly VisualTreeAsset _rowTemplate;
    private readonly int _listViewHeight;
    private readonly Action<PackageRowPresentationModel> _selectionChangeHandler;

    private string? _selectedPackageId;

    public string Title
    {
      get => _titleLabel.text;
      set => _titleLabel.text = value;
    }

    public string SelectedPackageId
    {
      set
      {
        if (_selectedPackageId == value)
          return;

        _selectedPackageId = value;
        var package = _listView.itemsSource.Cast<PackageRowPresentationModel>().First(i => i.Id == _selectedPackageId);
        _listView.SetSelectionWithoutNotify(new []{ _listView.itemsSource.IndexOf(package) });
      }
    }

    public void Refresh(List<PackageRowPresentationModel> sourceItems)
    {
      _listView.itemsSource = sourceItems;
      _listView.Refresh();
    }

    private void Initialize(IList sourceItems)
    {
      _listView.style.height = new StyleLength(_listViewHeight);
      _listView.selectionType = SelectionType.Single;
      _listView.itemHeight = ItemHeight;
      _listView.makeItem += () => _rowTemplate.CloneTree();
      _listView.itemsSource = sourceItems;
      _listView.bindItem += (element, i) =>
      {
        var model = (PackageRowPresentationModel)_listView.itemsSource[i];
        element.Q<Image>("Icon").image = model.Icon;
        element.Q<Label>("Id").text = model.Id;
        element.Q<Label>("Sources").text = string.Join(", " , model.Sources);
        element.Q<Label>("Version").text = model.Version;
      };
      _listView.onSelectionChange += objects =>
      {
        var selected = objects.Cast<PackageRowPresentationModel>().First();
        _selectionChangeHandler.Invoke(selected);
      };
    }

    public PackagesListControl(VisualElement parent, string title, int listViewHeight, List<PackageRowPresentationModel> sourceItems,
      Action<PackageRowPresentationModel> selectionChangeHandler)
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackagesListControlLayout);
      var root = visualTree.CloneTree();
      parent.Add(root);

      _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackageRowLayout);
      _titleLabel = root.Q<Label>("Title");
      _listView = root.Q<ListView>("ListView");

      _listViewHeight = listViewHeight;
      _selectionChangeHandler = selectionChangeHandler;
      Title = title;

      Initialize(sourceItems);
    }
  }
}
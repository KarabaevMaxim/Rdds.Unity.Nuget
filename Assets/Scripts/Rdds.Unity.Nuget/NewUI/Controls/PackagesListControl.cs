﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rdds.Unity.Nuget.NewUI.Controls.Models;
using UnityEditor;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.NewUI.Controls
{
  internal class PackagesListControl
  {
    private const int ItemHeight = 30;
    
    private readonly Label _titleLabel;
    private readonly ListView _listView;
    
    private readonly VisualTreeAsset _rowTemplate;
    private readonly int _listViewHeight;
    private readonly List<PackageRowPresentationModel> _sourceItems;
    private readonly Action<PackageRowPresentationModel> _selectionChangeHandler;

    public string Title
    {
      get => _titleLabel.text;
      set => _titleLabel.text = value;
    }

    private void Initialize()
    {
      _listView.style.height = new StyleLength(_listViewHeight);
      _listView.selectionType = SelectionType.Single;
      _listView.itemHeight = ItemHeight;
      _listView.makeItem += () => _rowTemplate.CloneTree();
      _listView.itemsSource = _sourceItems;
      _listView.bindItem += (element, i) =>
      {
        element.Q<Image>("Icon").image = _sourceItems[i].Icon;
        element.Q<Label>("Id").text = _sourceItems[i].Id;
        element.Q<Label>("Sources").text = string.Join(", " , _sourceItems[i].Sources);
        element.Q<Label>("Version").text = _sourceItems[i].Version;
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
      _sourceItems = sourceItems;
      _selectionChangeHandler = selectionChangeHandler;
      Title = title;

      Initialize();
    }
  }
}
﻿using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI
{
  public class PackageControl
  {
    private readonly Label _titleLbl;
    private readonly Image _iconImage;
    private readonly Label _descriptionLbl;
    private readonly Label _versionLabel;
    private readonly Label _downloadCountLabel;
    private readonly Button _downloadButton;

    private readonly PackageInfo _packageInfo;
    private readonly NugetService _service;

    public async Task SetFields()
    {
      _titleLbl.text = _packageInfo.Title;
      _descriptionLbl.text = _packageInfo.Description;
      _versionLabel.text = $"{_packageInfo.Identity.Version.OriginalString}";
      _downloadCountLabel.text = $"Downloaded {_packageInfo.DownloadCount?.ToString() ?? "-"} times";
      var icon = await DownloadHelper.DownloadImageAsync(_packageInfo.IconUrl);
      
      if (icon == null)
        icon = Resources.Load<Texture2D>("NugetIcon");

      _iconImage.image = icon;
      _downloadButton.clickable.clicked += async () => await _service.DownloadPackageAsync(_packageInfo.Identity, CancellationToken.None);
    }
    
    public PackageControl(VisualElement parent, PackageInfo info, NugetService service)
    {
      _packageInfo = info;
      _service = service;
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/PackageControl.uxml");
      var root = visualTree.CloneTree();
      parent.Add(root);
      _titleLbl = root.Q<Label>("TitleLbl");
      _descriptionLbl = root.Q<Label>("DescriptionLbl");
      _iconImage = root.Q<Image>("IconImage");
      _versionLabel = root.Q<Label>("VersionLabel");
      _downloadCountLabel = root.Q<Label>("DownloadCountLabel");
      _downloadButton = root.Q<Button>("DownloadButton");
    }
  }
}
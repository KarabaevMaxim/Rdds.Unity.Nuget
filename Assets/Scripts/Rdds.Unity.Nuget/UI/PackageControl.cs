using System.Threading.Tasks;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI
{
  public class PackageControl
  {
    private readonly Label _titleLbl;
    private readonly Image _iconImage;
    private readonly Label _descriptionLbl;
    private readonly Label _versionLabel;
    private readonly Label _downloadCountLabel;

    private readonly PackageInfo _packageInfo;

    public async Task SetFields()
    {
      _titleLbl.text = _packageInfo.Title;
      _descriptionLbl.text = _packageInfo.Description;
      _versionLabel.text = $"{_packageInfo.Version}";
      _downloadCountLabel.text = $"Downloaded {_packageInfo.DownloadCount?.ToString() ?? "-"} times";
      var icon = await DownloadHelper.DownloadImageAsync(_packageInfo.IconUrl);
      
      if (icon == null)
        icon = Resources.Load<Texture2D>("NugetIcon");

      _iconImage.image = icon;
    }
    
    public PackageControl(VisualElement parent, PackageInfo info)
    {
      _packageInfo = info;
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/PackageControl.uxml");
      var root = visualTree.CloneTree();
      parent.Add(root);
      _titleLbl = root.Q<Label>("TitleLbl");
      _descriptionLbl = root.Q<Label>("DescriptionLbl");
      _iconImage = root.Q<Image>("IconImage");
      _versionLabel = root.Q<Label>("VersionLabel");
      _downloadCountLabel = root.Q<Label>("DownloadCountLabel");
    }
  }
}
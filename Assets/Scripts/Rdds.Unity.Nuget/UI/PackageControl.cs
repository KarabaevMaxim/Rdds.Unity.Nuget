using Rdds.Unity.Nuget.Services;
using UnityEditor;
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

    public PackageControl(VisualElement parent, PackageInfo info)
    {
      var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/PackageControl.uxml");
      var root = visualTree.CloneTree();
      parent.Add(root);
      _titleLbl = root.Q<Label>("TitleLbl");
      _descriptionLbl = root.Q<Label>("DescriptionLbl");
      _iconImage = root.Q<Image>("IconImage");
      _versionLabel = root.Q<Label>("VersionLabel");
      _downloadCountLabel = root.Q<Label>("DownloadCountLabel");

      _titleLbl.text = info.Title;
      _descriptionLbl.text = info.Description;
      _iconImage.image = DownloadHelper.DownloadImage(info.IconUrl.AbsoluteUri);
      _versionLabel.text = $"{info.Version}";
      _downloadCountLabel.text = $"Downloaded {info.DownloadCount?.ToString() ?? "-"} times";
    }
  }
}
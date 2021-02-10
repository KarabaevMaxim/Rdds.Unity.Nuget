using UnityEditor;
using UnityEngine.UIElements;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class PackageControlBase
  {
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly Label _dependenciesLabel;

    protected PackageInfo PackageInfo { get; set; }
    protected VisualElement VersionPlaceholder { get; }
    protected Button ActionButton { get; }
    protected Image IconImage { get; }

    protected string Title
    {
      get => _titleLabel.text;
      set => _titleLabel.text = value;
    }
    
    protected string Description
    {
      get => _descriptionLabel.text;
      set => _descriptionLabel.text = value;
    }
    
    protected string Dependencies
    {
      get => _dependenciesLabel.text;
      set => _dependenciesLabel.text = value;
    }
    
    protected string ActionButtonText
    {
      get => ActionButton.text;
      set => ActionButton.text = value;
    }

    public PackageControlBase(VisualElement parent, PackageInfo packageInfo)
    {
      PackageInfo = packageInfo;
      var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackageControlLayout).CloneTree();
      parent.Add(root);
      VersionPlaceholder = root.Q<VisualElement>("VersionPlaceholder");
      _titleLabel = root.Q<Label>("TitleLbl");
      _descriptionLabel = root.Q<Label>("DescriptionLbl");
      IconImage = root.Q<Image>("IconImage");
      ActionButton = root.Q<Button>("ActionButton");
      _dependenciesLabel = root.Q<Label>("DependenciesLabel");
    }
  }
}
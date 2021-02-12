using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class PackageControlBase
  {
    private readonly VisualElement _root;
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly Label _dependenciesLabel;
    private readonly Image _iconImage;

    protected PackageInfo PackageInfo { get; set; }
    protected VisualElement VersionPlaceholder { get; }
    protected Button ActionButton { get; }

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

    protected string ActionButtonText
    {
      get => ActionButton.text;
      set => ActionButton.text = value;
    }

    protected Texture? Icon
    {
      get => _iconImage.image;
      // ReSharper disable once Unity.NoNullCoalescing
      set => _iconImage.image = value ?? Resources.Load<Texture2D>("NugetIcon");
    }
    
    private string Dependencies
    {
      get => _dependenciesLabel.text;
      set => _dependenciesLabel.text = value;
    }
    
    protected void RemoveFromLayout() => _root.RemoveFromHierarchy();

    protected async Task SetIconAsync(CancellationToken cancellationToken) =>
      Icon = PackageInfo.IconPath == null 
        ? null 
        : await ImageHelper.LoadImageAsync(PackageInfo.IconPath, cancellationToken);

    protected void SetDependencies()
    {
      if (PackageInfo.Dependencies == null || !PackageInfo.Dependencies.Any())
      {
        Dependencies = "No dependencies found";
        return;
      }
      
      var textDependencies = new StringBuilder();

      foreach (var group in PackageInfo.Dependencies)
      {
        textDependencies.AppendLine(group.TargetFramework.Name);
      
        foreach (var dependency in group.Dependencies)
          textDependencies.AppendLine($"  -{dependency.Id} >= {dependency.Version}");
      }

      Dependencies = textDependencies.ToString();
    }

    public PackageControlBase(VisualElement parent, PackageInfo packageInfo)
    {
      PackageInfo = packageInfo;
      _root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Paths.PackageControlLayout).CloneTree();
      parent.Add(_root);
      VersionPlaceholder = _root.Q<VisualElement>("VersionPlaceholder");
      _titleLabel = _root.Q<Label>("TitleLbl");
      _descriptionLabel = _root.Q<Label>("DescriptionLbl");
      _iconImage = _root.Q<Image>("IconImage");
      ActionButton = _root.Q<Button>("ActionButton");
      _dependenciesLabel = _root.Q<Label>("DependenciesLabel");

      Title = packageInfo.Title ?? packageInfo.Identity.Id;
      Description = PackageInfo.Description ?? "No description";
    }
  }
}
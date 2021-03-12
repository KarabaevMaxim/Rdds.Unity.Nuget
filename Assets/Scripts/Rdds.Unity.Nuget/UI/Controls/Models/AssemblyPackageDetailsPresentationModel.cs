using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI.Controls.Models
{
  /// <summary>
  /// Presentation model for assembly row in details panel of package.
  /// </summary>
  internal struct AssemblyPackageDetailsPresentationModel
  {
    [JsonIgnore]
    public Texture2D Icon { get; private set; }
    
    [UsedImplicitly]
    public string IconBase64
    {
      get => ImageHelper.TextureToBase64(Icon);
      set => Icon = ImageHelper.TextureFromBase64(value);
    }

    public string Name { get; }
    
    public string? InstalledVersionOfPackage { get; }
    
    [JsonIgnore]
    public Texture2D ButtonIcon { get; private set; }
    
    [UsedImplicitly]
    public string ButtonIconBase64
    {
      get => ImageHelper.TextureToBase64(ButtonIcon);
      set => ButtonIcon = ImageHelper.TextureFromBase64(value);
    }

    public Action ButtonAction { get; }

    public AssemblyPackageDetailsPresentationModel(Texture2D icon, string name, string? installedVersionOfPackage, Texture2D buttonIcon, Action buttonAction)
    {
      Icon = icon;
      Name = name;
      InstalledVersionOfPackage = installedVersionOfPackage;
      ButtonIcon = buttonIcon;
      ButtonAction = buttonAction;
    }
  }
}
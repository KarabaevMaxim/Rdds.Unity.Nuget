using System;
using UnityEngine;

namespace Rdds.Unity.Nuget.NewUI.Controls.Models
{
  /// <summary>
  /// Presentation model for assembly row in details panel of package.
  /// </summary>
  internal readonly struct AssemblyPackageDetailsPresentationModel
  {
    public Texture Icon { get; }
    
    public string Name { get; }
    
    public string? InstalledVersionOfPackage { get; }
    
    public Texture ButtonIcon { get; }
    
    public Action ButtonAction { get; }

    public AssemblyPackageDetailsPresentationModel(Texture icon, string name, string? installedVersionOfPackage, Texture buttonIcon, Action buttonAction)
    {
      Icon = icon;
      Name = name;
      InstalledVersionOfPackage = installedVersionOfPackage;
      ButtonIcon = buttonIcon;
      ButtonAction = buttonAction;
    }
  }
}
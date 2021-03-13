using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI.Controls.Models
{
  internal struct PackageDetailsPresentationModel
  {
    public string Id { get; }
    
    public bool IsInstalled { get; }
    
    [JsonIgnore]
    public Texture2D Icon { get; private set; }

    [UsedImplicitly]
    public string IconBase64
    {
      get => ImageHelper.TextureToBase64(Icon);
      set => Icon = ImageHelper.TextureFromBase64(value);
    }

    public string SelectedVersion { get; }
    
    public IEnumerable<string> Versions { get; set; }
    
    public string SelectedSource { get; }
    
    public IEnumerable<string> AvailableInSources { get; }
    
    public string? Description { get; set; }

    public IEnumerable<DependenciesPresentationModel>? Dependencies { get; set; }
    
    [JsonIgnore]
    public Texture2D InstallRemoveButtonIcon { get; private set; }
    
    [UsedImplicitly]
    public string InstallRemoveButtonIconBase64
    {
      get => ImageHelper.TextureToBase64(InstallRemoveButtonIcon);
      set => InstallRemoveButtonIcon = ImageHelper.TextureFromBase64(value);
    }

    public IEnumerable<AssemblyPackageDetailsPresentationModel> Assemblies { get; }
    
    public Action? InstallRemoveButtonAction { get; }
    
    public Action? UpdateButtonAction { get; }

    public PackageDetailsPresentationModel(string id, 
      bool isInstalled,
      Texture2D icon, 
      string selectedVersion, 
      IEnumerable<string> versions,
      string selectedSource, 
      IEnumerable<string> availableInSources, 
      string? description,
      IEnumerable<DependenciesPresentationModel>? dependencies, 
      Texture2D installRemoveButtonIcon,
      Action? installRemoveButtonAction, 
      Action? updateButtonAction,
      IEnumerable<AssemblyPackageDetailsPresentationModel> assemblies)
    {
      Id = id;
      IsInstalled = isInstalled;
      Icon = icon;
      SelectedVersion = selectedVersion;
      Versions = versions;
      SelectedSource = selectedSource;
      AvailableInSources = availableInSources;
      Description = description;
      Dependencies = dependencies;
      InstallRemoveButtonIcon = installRemoveButtonIcon;
      InstallRemoveButtonAction = installRemoveButtonAction;
      UpdateButtonAction = updateButtonAction;
      Assemblies = assemblies;
    }
  }
}
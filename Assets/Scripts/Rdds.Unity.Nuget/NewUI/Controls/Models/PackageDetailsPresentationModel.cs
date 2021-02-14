using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rdds.Unity.Nuget.NewUI.Controls.Models
{
  internal struct PackageDetailsPresentationModel
  {
    public string Id { get; }
    
    public Texture Icon { get; }
    
    public string SelectedVersion { get; }
    
    public List<string> Versions { get; set; }
    
    public string SelectedSource { get; }
    
    public List<string> AvailableInSources { get; }
    
    public string? Description { get; set; }

    public IEnumerable<DependenciesPresentationModel>? Dependencies { get; set; }
    
    public Texture InstallRemoveButtonIcon { get; }
    
    public List<AssemblyPackageDetailsPresentationModel> Assemblies { get; }
    
    public Action? InstallRemoveButtonAction { get; }
    
    public Action? UpdateButtonAction { get; }

    public PackageDetailsPresentationModel(string id, Texture icon, string selectedVersion, List<string> versions,
      string selectedSource, List<string> availableInSources, string? description,
      IEnumerable<DependenciesPresentationModel>? dependencies, Texture installRemoveButtonIcon,
      Action? installRemoveButtonAction, Action? updateButtonAction,
      List<AssemblyPackageDetailsPresentationModel> assemblies)
    {
      Id = id;
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
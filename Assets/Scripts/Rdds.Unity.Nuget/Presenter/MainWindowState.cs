using System.Collections.Generic;
using JetBrains.Annotations;
using Rdds.Unity.Nuget.UI.Controls.Models;

namespace Rdds.Unity.Nuget.Presenter
{
  internal struct MainWindowState
  {
    public LeftPanelState LeftPanelState { get; set; }

    public PackageDetailsPresentationModel? RightPanelState { get; set; }
  }
  
  internal struct LeftPanelState
  {
    public HeaderState Header { get; set; }

    public IEnumerable<PackageRowPresentationModel> InstalledPackagesList { get; set; }
    
    public IEnumerable<PackageRowPresentationModel> AvailablePackagesList { get; set; }
    
    internal struct HeaderState
    {
      public string FilterString { get; set; }

      public IEnumerable<string> SourcesList { get; set; }

      public string SelectedSource { get; set; }

      public string SelectedAssembly { get; set; }

      public IEnumerable<string> AssembliesList { get; set; }
    }
  }

  // internal struct RightPanelState
  // {
  //   public string Id { get; set; }
  //   
  //   public byte[] IconBytes { get; set; }
  //   
  //   public string SelectedVersion { get; set; }
  //   
  //   public IEnumerable<string> Versions { get; set; }
  //   
  //   public string SelectedSource { get; set; }
  //   
  //   public IEnumerable<string> AvailableInSources { get; set; }
  //   
  //   public string? Description { get; set; }
  //
  //   public IEnumerable<DependenciesPresentationModel>? Dependencies { get; set; }
  //   
  //   public byte[] InstallRemoveButtonIconBytes { get; set; }
  //   
  //   public IEnumerable<AssemblyPackageDetailsPresentationModel> Assemblies { get; set; }
  //
  //   // todo remove
  //   // public RightPanelState? FromPresentationModel(PackageDetailsPresentationModel? model)
  //   // {
  //   //   if (model == null)
  //   //     return null;
  //   //
  //   //   return new RightPanelState
  //   //   {
  //   //     Id = model.Value.Id,
  //   //     IconBytes = model.Value.Icon.GetRawTextureData(),
  //   //     SelectedVersion = model.Value.SelectedVersion,
  //   //     Versions = model.Value.Versions,
  //   //     SelectedSource = model.Value.SelectedSource,
  //   //     AvailableInSources = model.Value.AvailableInSources,
  //   //     Description = model.Value.Description,
  //   //     Dependencies = model.Value.Dependencies,
  //   //     InstallRemoveButtonIconBytes = model.Value.InstallRemoveButtonIcon.GetRawTextureData(),
  //   //     Assemblies = model.Value.Assemblies
  //   //   };
  //   //
  //   // }
  // }
}
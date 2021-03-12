using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.UI.Controls.Models;

namespace Rdds.Unity.Nuget.UI
{
  internal interface IMainWindow
  {
    IEnumerable<PackageRowPresentationModel> InstalledPackages { get; set; }
    
    IEnumerable<PackageRowPresentationModel> AvailablePackages { get; set; }
    
    PackageDetailsPresentationModel? SelectedPackage { get; set; }

    IEnumerable<string> Sources { get; set; }
    
    IEnumerable<string> Assemblies { get; set; }
    
    bool IsListsLoading { set; }
    
    bool IsDetailsLoading { get; set; }
    
    string? DetailsSelectedSource { get; }
    
    string? DetailsSelectedVersion { get; }

    string FilterString { get; set; }

    string SelectedAssembly { get; set; }
    
    string SelectedSource { get; set; }

    event Action<PackageRowPresentationModel>? PackageRowSelected;
    event Action<string>? FilterTextChanged;
    event Action<string>? AssemblyChanged;
    event Action<string>? SourceChanged;
    event Action? WillDisabled;
  }
}
using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.New.UI.Controls;
using Rdds.Unity.Nuget.New.UI.Controls.Models;

namespace Rdds.Unity.Nuget.New.UI
{
  internal interface IMainWindow
  {
    List<PackageRowPresentationModel> InstalledPackages { get; set; }
    
    List<PackageRowPresentationModel> AvailablePackages { get; set; }
    
    PackageDetailsControl PackageDetailsControl { get; }
    
    public List<string> Sources { get; set; }
    
    public List<string> Assemblies { get; set; }

    event Action<PackageRowPresentationModel>? PackageRowSelected;
    
    event Action<string>? FilterTextChanged;

    event Action<string>? AssemblyChanged;
    
    event Action<string>? SourceChanged;
    
    event Action? WindowPostEnabled;
  }
}
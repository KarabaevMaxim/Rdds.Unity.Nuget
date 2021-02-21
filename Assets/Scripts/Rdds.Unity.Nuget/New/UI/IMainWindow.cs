using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.New.UI.Controls;
using Rdds.Unity.Nuget.New.UI.Controls.Models;

namespace Rdds.Unity.Nuget.New.UI
{
  internal interface IMainWindow
  {
    IEnumerable<PackageRowPresentationModel> InstalledPackages { get; set; }
    
    IEnumerable<PackageRowPresentationModel> AvailablePackages { get; set; }
    
    PackageDetailsControl PackageDetailsControl { get; }
    
    public IEnumerable<string> Sources { get; set; }
    
    public IEnumerable<string> Assemblies { get; set; }

    event Action<PackageRowPresentationModel>? PackageRowSelected;
    
    event Action<string>? FilterTextChanged;

    event Action<string>? AssemblyChanged;
    
    event Action<string>? SourceChanged;
  }
}
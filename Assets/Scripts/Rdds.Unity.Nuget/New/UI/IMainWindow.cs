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
    
    IEnumerable<string> Sources { get; set; }
    
    IEnumerable<string> Assemblies { get; set; }

    void SetSource(string key);
    
    event Action<PackageRowPresentationModel>? PackageRowSelected;
    
    event Action<string>? FilterTextChanged;

    event Action<string>? AssemblyChanged;
    
    event Action<string>? SourceChanged;
  }
}
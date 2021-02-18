using System.Collections.Generic;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.UI.Controls.Models
{
  internal readonly struct PackageRowPresentationModel
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public Texture Icon { get; }

    public List<string> Sources { get; }
    
    public IEnumerable<string> InstalledInAssemblies { get; }

    public PackageRowPresentationModel(string id, string version, Texture icon, List<string> sources, IEnumerable<string> installedInAssemblies)
    {
      Icon = icon;
      Id = id;
      Version = version;
      Sources = sources;
      InstalledInAssemblies = installedInAssemblies;
    }
  }
}
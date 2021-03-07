using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rdds.Unity.Nuget.New.UI.Controls.Models
{
  internal readonly struct PackageRowPresentationModel
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public Texture Icon { get; }

    public IEnumerable<string> Sources { get; }
    
    public IEnumerable<string> InstalledInAssemblies { get; }

    public static PackageRowPresentationModel Default(Texture icon) =>
      new PackageRowPresentationModel("none", "none", icon, Enumerable.Empty<string>(), Enumerable.Empty<string>());

    public PackageRowPresentationModel(string id, string version, Texture icon, IEnumerable<string> sources, IEnumerable<string> installedInAssemblies)
    {
      Icon = icon;
      Id = id;
      Version = version;
      Sources = sources;
      InstalledInAssemblies = installedInAssemblies;
    }
  }
}
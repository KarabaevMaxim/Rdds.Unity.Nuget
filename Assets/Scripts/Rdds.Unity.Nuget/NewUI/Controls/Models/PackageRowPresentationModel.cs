using System.Collections.Generic;
using UnityEngine;

namespace Rdds.Unity.Nuget.NewUI.Controls.Models
{
  internal readonly struct PackageRowPresentationModel
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public Texture Icon { get; }

    public List<string> Sources { get; }

    public PackageRowPresentationModel(string id, string version, Texture icon, List<string> sources)
    {
      Icon = icon;
      Id = id;
      Version = version;
      Sources = sources;
    }
  }
}
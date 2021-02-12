using System.Collections.Generic;
using UnityEngine;

namespace Rdds.Unity.Nuget.NewUI.Controls
{
  public readonly struct PackageRowPresentationModel
  {
    public Texture Icon { get; }
    
    public string Id { get; }
    
    public string Version { get; }
    
    public IEnumerable<string> Sources { get; }

    public PackageRowPresentationModel(string id, string version, Texture icon, IEnumerable<string> sources)
    {
      Id = id;
      Version = version;
      Icon = icon;
      Sources = sources;
    }
  }
}
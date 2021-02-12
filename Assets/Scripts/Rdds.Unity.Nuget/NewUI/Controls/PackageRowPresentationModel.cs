using System.Collections.Generic;
using UnityEngine;

namespace Rdds.Unity.Nuget.NewUI.Controls
{
  public struct PackageRowPresentationModel
  {
    public Texture Texture { get; set; }
    
    public string Id { get; set; }
    
    public string Version { get; set; }
    
    public IEnumerable<string> Sources { get; set; }
  }
}
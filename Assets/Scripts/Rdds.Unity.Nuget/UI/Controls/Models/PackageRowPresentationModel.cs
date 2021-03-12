using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Utility;
using UnityEngine;

namespace Rdds.Unity.Nuget.UI.Controls.Models
{
  internal struct PackageRowPresentationModel
  {
    public string Id { get; }
    
    public string Version { get; }
    
    [JsonIgnore]
    public Texture2D Icon { get; private set; }

    [UsedImplicitly]
    public string IconBase64
    {
      get => ImageHelper.TextureToBase64(Icon);
      set => Icon = ImageHelper.TextureFromBase64(value);
    }

    public IEnumerable<string> Sources { get; }
    
    public IEnumerable<string> InstalledInAssemblies { get; }

    public static PackageRowPresentationModel Default(Texture2D icon) =>
      new PackageRowPresentationModel("none", "none", icon, Enumerable.Empty<string>(), Enumerable.Empty<string>());

    public PackageRowPresentationModel(string id, string version, Texture2D icon, IEnumerable<string> sources, IEnumerable<string> installedInAssemblies)
    {
      Icon = icon;
      Id = id;
      Version = version;
      Sources = sources;
      InstalledInAssemblies = installedInAssemblies;
    }
  }
}
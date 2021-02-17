using System.Collections.Generic;
using System.Linq;
using Rdds.Unity.Nuget.NewUI.Controls.Models;
using Rdds.Unity.Nuget.Other;

namespace Rdds.Unity.Nuget.NewUI
{
  internal static class PackagesFiltrationHelper
  {
    public static IEnumerable<PackageRowPresentationModel> ShowedInstalledPackages { get; set; } =
      new List<PackageRowPresentationModel>(0);
    
    public static IEnumerable<PackageRowPresentationModel> FilterByPartId(string idPart)
    {
      ShowedInstalledPackages = ShowedInstalledPackages.Where(p => StringExtensions.ContainsIgnoreCase(p.Id, idPart));
      return ShowedInstalledPackages;
    }

    public static IEnumerable<PackageRowPresentationModel> FilterByAssembly(string assembly)
    {
      ShowedInstalledPackages = ShowedInstalledPackages.Where(p => p.InstalledInAssemblies.Contains(assembly));
      return ShowedInstalledPackages;
    }

    public static IEnumerable<PackageRowPresentationModel> FilterBySource(
      IEnumerable<PackageRowPresentationModel> sourceCollection, string source)
    {
      ShowedInstalledPackages = sourceCollection.Where(p => p.Sources.Contains(source));
      return ShowedInstalledPackages;
    }
  }
}
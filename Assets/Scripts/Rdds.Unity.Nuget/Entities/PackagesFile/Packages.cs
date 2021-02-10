using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities.PackagesFile
{
  public class Packages
  {
    public List<Package> PackagesList { get; }
    
    public Packages(List<Package> packagesList) => PackagesList = packagesList;
  }
}
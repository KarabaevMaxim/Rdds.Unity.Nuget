using System;

namespace Rdds.Unity.Nuget.New.Exceptions
{
  public class PackageNotFoundException : Exception
  {
    public PackageNotFoundException(string packageId, string version) : base($"Package {packageId} {version} not found")
    {
    }
  }
}
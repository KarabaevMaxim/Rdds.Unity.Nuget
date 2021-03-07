using System;

namespace Rdds.Unity.Nuget.Exceptions
{
  public class PackageNotFoundException : Exception
  {
    public PackageNotFoundException(string packageId, string version) : base($"Package {packageId} {version} not found")
    {
    }
  }
}
using System;

namespace Rdds.Unity.Nuget.Exceptions
{
  public class PackageNotInstalledException : Exception
  {
    public PackageNotInstalledException(string packageId) : base($"Package {packageId} not installed")
    {
    }
  }
}
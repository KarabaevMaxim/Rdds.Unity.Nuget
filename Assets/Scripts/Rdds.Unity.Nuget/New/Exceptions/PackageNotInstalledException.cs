using System;

namespace Rdds.Unity.Nuget.New.Exceptions
{
  public class PackageNotInstalledException : Exception
  {
    public PackageNotInstalledException(string packageId) : base($"Package {packageId} not installed")
    {
    }
  }
}
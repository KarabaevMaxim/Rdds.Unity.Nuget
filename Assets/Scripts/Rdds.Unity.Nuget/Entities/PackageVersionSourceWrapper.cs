namespace Rdds.Unity.Nuget.Entities
{
  internal readonly struct PackageVersionSourceWrapper
  {
    public PackageVersion PackageVersion { get; }
    
    public string AvailableInSource { get; }

    public PackageVersionSourceWrapper(PackageVersion packageVersion, string availableInSource)
    {
      PackageVersion = packageVersion;
      AvailableInSource = availableInSource;
    }
  }
}
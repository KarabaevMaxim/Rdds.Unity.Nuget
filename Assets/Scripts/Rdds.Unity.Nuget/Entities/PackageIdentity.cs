namespace Rdds.Unity.Nuget.Entities
{
  public class PackageIdentity
  {
    public string Id { get; }
    
    public PackageVersion Version { get; }

    public PackageIdentity(string id, PackageVersion version)
    {
      Id = id;
      Version = version;
    }

    public PackageIdentity(string id, string version)
    {
      Id = id;
      Version = PackageVersion.Parse(version);
    }
  }
}
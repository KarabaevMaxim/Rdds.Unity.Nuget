namespace Rdds.Unity.Nuget.Entities
{
  public class PackageIdentity
  {
    public string Id { get; }
    
    public PackageVersion Version { get; }

    public override string ToString() => $"{Id}.{Version}";

    public override bool Equals(object obj) => 
      ReferenceEquals(this, obj) || (obj is PackageIdentity other && Equals(other));

    private bool Equals(PackageIdentity other) => Id == other.Id && Version.Equals(other.Version);

    public override int GetHashCode()
    {
      unchecked
      {
        return (Id.GetHashCode() * 397) ^ Version.GetHashCode();
      }
    }

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
namespace Rdds.Unity.Nuget.Entities.PackagesFile
{
  public class Package
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public string Source { get; }
    
    public string Path { get; }

    public Package(string id, string version, string source, string path)
    {
      Id = id;
      Version = version;
      Source = source;
      Path = path;
    }
  }
}
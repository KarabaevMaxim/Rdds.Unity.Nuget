namespace Rdds.Unity.Nuget.New.Services.Configs.Models
{
  public class LocalPackageInfo
  {
    public string Id { get; }
    
    public string Version { get; }
    
    public string Path { get; }

    public LocalPackageInfo(string id, string version, string path)
    {
      Id = id;
      Version = version;
      Path = path;
    }
  }
}
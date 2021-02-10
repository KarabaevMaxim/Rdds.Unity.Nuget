namespace Rdds.Unity.Nuget.Entities.NugetConfig
{
  public class NugetPackageSource
  {
    public string Key { get; set; } = null!;
    
    public string Path { get; set; } = null!;
    
    public Credentials? Credentials { get; set; }
  }
}
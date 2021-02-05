namespace Rdds.Unity.Nuget.Entities
{
  public class PackageVersion
  {
    public string OriginalString { get; set; } = null!;
    
    public int Major { get; set; }
    
    public int Minor { get; set; }
    
    public int Patch { get; set; }

    public override string ToString()
    {
      return $"{Major}.{Minor}.{Patch}";
    }
  }
}
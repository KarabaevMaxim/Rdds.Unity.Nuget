namespace Rdds.Unity.Nuget.Entities.NugetConfig
{
  public class Credentials
  {
    public string Username { get; set; } = null!;
    
    public string? Password { get; set; } = null!;

    public bool IsPasswordClearText { get; set; } = true;
  }
}
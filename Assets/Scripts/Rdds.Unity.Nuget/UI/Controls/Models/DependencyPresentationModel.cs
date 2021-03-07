namespace Rdds.Unity.Nuget.UI.Controls.Models
{
  internal class DependencyPresentationModel
  {
    public string Name { get; }
    
    public string MinVersion { get; }

    public DependencyPresentationModel(string name, string minVersion)
    {
      Name = name;
      MinVersion = minVersion;
    }
  }
}
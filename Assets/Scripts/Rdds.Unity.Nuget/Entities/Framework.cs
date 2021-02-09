using NuGet.Frameworks;

namespace Rdds.Unity.Nuget.Entities
{
  public class Framework
  {
    public string Name { get; }

    public NuGetFramework ToNugetFramework() => NuGetFramework.Parse(Name);

    public Framework(string name) => Name = name;
  }
}
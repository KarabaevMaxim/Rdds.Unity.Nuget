using System.Collections.Generic;

namespace Rdds.Unity.Nuget.NewUI.Controls.Models
{
  internal class DependenciesPresentationModel
  {
    public string TargetFramework { get; }
    
    public IEnumerable<DependencyPresentationModel> Dependencies { get; }

    public DependenciesPresentationModel(string targetFramework, IEnumerable<DependencyPresentationModel> dependencies)
    {
      TargetFramework = targetFramework;
      Dependencies = dependencies;
    }
  }
}
using System;
using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  [Serializable]
  public class PackageInfo
  {
    public string Title { get; set; }
    
    public string Authors { get; set; }
    
    public string Description { get; set; }
    
    public Uri IconUrl { get; set; }
    
    public string Owners { get; set; }
    
    public string Summary { get; set; }
    
    public long? DownloadCount { get; set; }
    
    public PackageIdentity Identity { get; set; }
    
    public IEnumerable<FrameworkGroup> Dependencies { get; set; }
  }
}
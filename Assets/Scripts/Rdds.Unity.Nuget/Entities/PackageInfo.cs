using System;
using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  [Serializable]
  public class PackageInfo
  {
    public string Title { get; }
    
    public string Authors { get; }
    
    public string Description { get; }
    
    public Uri IconUrl { get; }
    
    public string Owners { get; }
    
    public string Summary { get; }
    
    public long? DownloadCount { get; }
    
    public PackageIdentity Identity { get; }
    
    public IEnumerable<FrameworkGroup>? Dependencies { get; set; }

    public PackageInfo(string title, string authors, string description, Uri iconUrl, string owners, string summary,
      long? downloadCount, PackageIdentity identity, IEnumerable<FrameworkGroup>? dependencies)
    {
      Title = title;
      Authors = authors;
      Description = description;
      IconUrl = iconUrl;
      Owners = owners;
      Summary = summary;
      DownloadCount = downloadCount;
      Identity = identity;
      Dependencies = dependencies;
    }
  }
}
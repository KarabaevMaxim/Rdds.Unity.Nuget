using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  internal class PackageInfo
  {
    public string? Title { get; }
    
    public string Authors { get; }
    
    public string? Description { get; }
    
    public ResourcePath? IconPath { get; }
    
    public string? Owners { get; }

    public long? DownloadCount { get; }
    
    public PackageIdentity Identity { get; }
    
    public IEnumerable<FrameworkGroup>? Dependencies { get; set; }

    public PackageInfo(string? title, string authors, string? description, string? iconPath, string? owners,
      long? downloadCount, PackageIdentity identity, IEnumerable<FrameworkGroup>? dependencies)
    {
      Title = title;
      Authors = authors;
      Description = description;
      IconPath = iconPath == null ? null : new ResourcePath(iconPath);
      Owners = owners;
      DownloadCount = downloadCount;
      Identity = identity;
      Dependencies = dependencies;
    }
  }
}
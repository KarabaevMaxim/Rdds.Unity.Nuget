using System;
using UnityEngine;

namespace Rdds.Unity.Nuget
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
    
    public string Id { get; set; }
    
    public string Version { get; set; }
  }
}
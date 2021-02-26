using System.Collections.Generic;

namespace Rdds.Unity.Nuget.Entities
{
  internal readonly struct PackageInfoSourceWrapper
  {
    public PackageInfo PackageInfo { get; }
    
    public IEnumerable<string> SourceKeys { get; }

    public PackageInfoSourceWrapper(PackageInfo packageInfo, IEnumerable<string> sourceKeys)
    {
      PackageInfo = packageInfo;
      SourceKeys = sourceKeys;
    }
  }
}
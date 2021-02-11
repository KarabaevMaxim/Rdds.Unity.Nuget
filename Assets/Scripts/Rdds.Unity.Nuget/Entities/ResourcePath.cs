using System;

namespace Rdds.Unity.Nuget.Entities
{
  internal class ResourcePath
  {
    public string Path { get; }

    public bool IsLocalPath => !Path.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ;

    public ResourcePath(string path) => Path = path;
  }
}
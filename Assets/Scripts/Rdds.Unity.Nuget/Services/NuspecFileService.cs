using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Common;
using Rdds.Unity.Nuget.Entities;
using PackageIdentity = Rdds.Unity.Nuget.Entities.PackageIdentity;

namespace Rdds.Unity.Nuget.Services
{
  internal class NuspecFileService
  {
    private readonly ILogger _logger;

    public PackageInfo? GetPackageInfoFromNuspec(string packageDirectoryPath)
    {
      var nuspecFiles = Directory.GetFiles(packageDirectoryPath, "*.nuspec", SearchOption.AllDirectories);

      if (nuspecFiles.Length == 0)
      {
        _logger.LogWarning($".nuspec file of package '{packageDirectoryPath}' not found!");
        return null;
      }
      
      if (nuspecFiles.Length > 1)
      {
        _logger.LogWarning($"Too many .nuspec files of package '{packageDirectoryPath}'!");
        return null;
      }

      return ParseNuspecFile(nuspecFiles[0]);
    }

    private PackageInfo ParseNuspecFile(string nuspecFile)
    {
      var xDoc = XDocument.Load(nuspecFile); 
      var root = xDoc.Root!; 
      var xNamespace = root.GetDefaultNamespace().NamespaceName;
      var meta = root.Element(XName.Get("metadata", xNamespace))!;
      var id = meta.Element(XName.Get("id", xNamespace))!.Value!;
      var version = meta.Element(XName.Get("version", xNamespace))!.Value!;
      var authors = meta.Element(XName.Get("authors", xNamespace))!.Value!;
      var owners = meta.Element(XName.Get("owners", xNamespace))?.Value;
      var iconPath = (meta.Element(XName.Get("iconUrl", xNamespace)) ?? meta.Element(XName.Get("icon", xNamespace)))?.Value;
      var description = meta.Element(XName.Get("description", xNamespace))?.Value;
      var title = meta.Element(XName.Get("title", xNamespace))?.Value;
      var dependenciesNode = meta.Element(XName.Get("dependencies", xNamespace));
      List<FrameworkGroup>? dependencies = null;
      
      if (dependenciesNode != null)
      {
        dependencies = new List<FrameworkGroup>();
        var groups = dependenciesNode.Elements(XName.Get("group", xNamespace));

        foreach (var group in groups)
        {
          var deps = group.Elements(XName.Get("dependency", xNamespace))
            .Select(e =>
            {
              var depId = e.Attribute(XName.Get("id", xNamespace))!.Value!;
              var depVer =  e.Attribute(XName.Get("version", xNamespace))!.Value!;
              return new PackageIdentity(depId, PackageVersion.Parse(depVer));
            });

          // attribute name without namespace
          var framework = group.Attribute("targetFramework")!.Value;
          dependencies.Add(new FrameworkGroup(new Framework(framework), deps));
        }
      }
      
      return new PackageInfo(title, authors, description, iconPath, owners, null,
        new PackageIdentity(id, PackageVersion.Parse(version)), dependencies);
    }

    public NuspecFileService(ILogger logger) => _logger = logger;
  }
}
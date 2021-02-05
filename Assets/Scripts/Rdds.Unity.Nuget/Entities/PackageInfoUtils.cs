using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Rdds.Unity.Nuget.Entities
{
  public static class PackageInfoUtils
  {
    public static PackageInfo ToPackageInfo(this IPackageSearchMetadata metadata)
    {
      return new PackageInfo
      {
        Authors = metadata.Authors,
        Description = metadata.Description,
        DownloadCount = metadata.DownloadCount,
        IconUrl = metadata.IconUrl,
        Owners = metadata.Owners,
        Summary = metadata.Summary,
        Title = metadata.Title,
        Identity = new PackageIdentity
        {
          Id = metadata.Identity.Id,
          Version = metadata.Identity.Version.ToPackageVersion()
        },
        Dependencies = metadata.DependencySets.Select(d => d.ToFrameworkGroup())
      };
    }

    public static FrameworkGroup ToFrameworkGroup(this PackageDependencyGroup dependency)
    {
      return new FrameworkGroup
      {
        TargetFramework = dependency.TargetFramework.GetFrameworkString(),
        Dependencies = dependency.Packages.Select(d => new PackageIdentity
        {
          Id = d.Id,
          Version = d.VersionRange.MaxVersion.ToPackageVersion()
        })
      };
    }

    public static PackageVersion ToPackageVersion(this NuGetVersion version)
    {
      return new PackageVersion
      {
        Major = version.Major,
        Minor = version.Minor,
        Patch = version.Patch,
        OriginalString = version.OriginalVersion
      };
    }

    public static NuGetVersion ToNugetVersion(this PackageVersion version)
    {
      return new NuGetVersion(version.ToString());
    }
  }
}
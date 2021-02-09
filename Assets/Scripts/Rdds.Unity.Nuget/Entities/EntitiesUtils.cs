using System.Linq;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Rdds.Unity.Nuget.Entities.Config;

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
        Identity = metadata.Identity.ToPackageIdentity(),
        Dependencies = metadata.DependencySets.Select(d => d.ToFrameworkGroup())
      };
    }

    public static PackageIdentity ToPackageIdentity(this NuGet.Packaging.Core.PackageIdentity source) => 
      new PackageIdentity(source.Id, source.Version.ToPackageVersion());

    public static NuGet.Packaging.Core.PackageIdentity ToNugetPackageIdentity(this PackageIdentity identity) => 
      new NuGet.Packaging.Core.PackageIdentity(identity.Id, identity.Version.ToNugetVersion());

    public static FrameworkGroup ToFrameworkGroup(this PackageDependencyGroup dependency) =>
      new FrameworkGroup
      {
        TargetFramework = dependency.TargetFramework.GetFrameworkString(),
        Dependencies = dependency.Packages
          .Select(d => new PackageIdentity(d.Id, d.VersionRange.MinVersion.ToPackageVersion()))
      };

    public static PackageVersion ToPackageVersion(this NuGetVersion version) => 
      new PackageVersion(version.Major, version.Minor, version.Patch, version.ReleaseLabels, version.OriginalVersion);

    public static NuGetVersion ToNugetVersion(this PackageVersion version) => new NuGetVersion(version.ToString());

    public static PackageSourceCredential? CreatePackageSourceCredentials(this NugetPackageSource packageSource) =>
      packageSource.Credentials == null 
        ? null 
        : new PackageSourceCredential(packageSource.Path, packageSource.Credentials!.Username,
          packageSource.Credentials!.Password, packageSource.Credentials.IsPasswordClearText, null);

    public static PackageSource ToPackageSource(this NugetPackageSource packageSource) =>
      new PackageSource(packageSource.Path)
      {
        Credentials = packageSource.CreatePackageSourceCredentials()
      };
  }
}
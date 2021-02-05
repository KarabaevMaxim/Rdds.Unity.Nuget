using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.Services
{
  public class NugetService
  {
    public async Task<IEnumerable<PackageInfo>> GetPackages(string filterString, int skip, int take)
    {
      var logger = NullLogger.Instance;
      var cancellationToken = CancellationToken.None;

      var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
      var resource = await repository.GetResourceAsync<PackageSearchResource>();
      var searchFilter = new SearchFilter(true);

      var results =
        await resource.SearchAsync(filterString, searchFilter, skip, take, logger, cancellationToken);

      return results.Select(r => r.ToPackageInfo());
    }

    public async Task<IEnumerable<PackageInfo>> GetPackages(string packageId)
    {
      var logger = NullLogger.Instance;
      var cancellationToken = CancellationToken.None;

      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
      var resource = await repository.GetResourceAsync<PackageMetadataResource>();

      var packages =
        await resource.GetMetadataAsync(packageId, true, false, cache, logger, cancellationToken);

      return packages.Select(p => p.ToPackageInfo());
    }

    public async Task DownloadPackage(PackageIdentity identity)
    {
      var logger = NullLogger.Instance;
      var cancellationToken = CancellationToken.None;
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
      var packageVersion = identity.Version.ToNugetVersion();
      using var packageStream = new MemoryStream();
      await resource.CopyNupkgToStreamAsync(identity.Id, packageVersion, packageStream, cache, logger, cancellationToken);
      packageStream.CopyToFile(Config.GetCacheNupkgFileName(identity));
    }

    public async Task<IEnumerable<PackageVersion>> GetPackageVersions(string packageId)
    {
      var logger = NullLogger.Instance;
      var cancellationToken = CancellationToken.None;
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
      var versions = await resource.GetAllVersionsAsync(packageId, cache, logger, cancellationToken);
      return versions.Select(v => v.ToPackageVersion());
    }
  }
}
  
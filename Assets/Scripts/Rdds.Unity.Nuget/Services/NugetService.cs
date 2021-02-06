using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;

namespace Rdds.Unity.Nuget.Services
{
  public class NugetService
  {
    private readonly ILogger _logger;
    private readonly NugetConfigService _configService;

    public NugetPackageSource SelectedSource { get; private set; }
    
    public void ChangeActiveSource(string key) => SelectedSource = _configService.GetPackageSource(key);
    
    public async Task<IEnumerable<PackageInfo>> GetPackagesAsync(string filterString, int skip, int take, CancellationToken cancellationToken)
    {
      var packageSource = SelectedSource.ToPackageSource();
      var repository = Repository.Factory.GetCoreV3(packageSource);
      var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
      var searchFilter = new SearchFilter(true);
      var results =
        await resource.SearchAsync(filterString, searchFilter, skip, take, _logger, cancellationToken);
      return results.Select(r => r.ToPackageInfo());
    }

    public async Task<IEnumerable<PackageInfo>> GetPackagesAsync(string packageId, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.Path);
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(packageId, true, false, cache, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo());
    }

    public async Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.Path);
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var versions = await resource.GetAllVersionsAsync(packageId, cache, _logger, cancellationToken);
      return versions.Select(v => v.ToPackageVersion());
    }

    public async Task<string> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.Path);
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var packageVersion = identity.Version.ToNugetVersion();
      using var packageStream = new MemoryStream();
      await resource.CopyNupkgToStreamAsync(identity.Id, packageVersion, packageStream, cache, _logger, cancellationToken);
      return packageStream.CopyToFile(Config.GetCacheNupkgFileName(identity));
    }
    
    public void InstallPackage(PackageInfo package)
    {
      
    }

    public void Initialize() => SelectedSource = _configService.GetDefaultPackageSource();
    
    public NugetService(ILogger logger, NugetConfigService configService)
    {
      _logger = logger;
      _configService = configService;

    }
  }
}
  
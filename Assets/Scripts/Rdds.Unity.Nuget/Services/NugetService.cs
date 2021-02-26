using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.NugetConfig;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Utility;
using PackageIdentity = Rdds.Unity.Nuget.Entities.PackageIdentity;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.Services
{
  internal class NugetService
  {
    private readonly ILogger _logger;
    private readonly NugetConfigService _configService;
    private readonly FileService _fileService;
    private readonly FrameworkService _frameworkService;
    
    public NugetPackageSource Source { get; }

    public async Task<IEnumerable<PackageInfo>> SearchPackagesAsync(string filterString, int skip, int take, CancellationToken cancellationToken)
    {
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
      var searchFilter = new SearchFilter(true);
      var foundPackages =
        await resource.SearchAsync(filterString, searchFilter, skip, take, _logger, cancellationToken);

      return foundPackages.Select(p => p.ToPackageInfo());
    }

    public async Task<IEnumerable<FrameworkGroup>> FindDependenciesAsync(PackageIdentity packageIdentity,
      CancellationToken cancellationToken)
    {
      var cacheContext = new SourceCacheContext();
      var repositories = new[] { Repository.Factory.GetCoreV3(Source.ToPackageSource()) };
      var currentFramework = _frameworkService.RequireCurrentFramework();
      var dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      
      await FindPackageDependenciesRecursive(
        packageIdentity.ToNugetPackageIdentity(),
        currentFramework.ToNugetFramework(),
        cacheContext,
        repositories,
        dependencies,
        cancellationToken);

      return new []
      {
        new FrameworkGroup(currentFramework, dependencies.Select(d => d.ToPackageIdentity()))
      };
    }

    public async Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(identity.Id, true, false, cache, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo()).FirstOrDefault(p => p.Identity.Version.Equals(identity.Version));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var versions = await resource.GetAllVersionsAsync(packageId, cache, _logger, cancellationToken);
      versions = versions.OrderByDescending(v => v);
      var last = versions.First();
      return versions.Where(v => !v.IsPrerelease || v == last).Select(v => v.ToPackageVersion());
    }

    public async Task<string?> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var version = identity.Version.ToNugetVersion();
      var tempFilePath = _fileService.GetNupkgTempFilePath(identity);

      using (var stream = _fileService.CreateWriteFileStream(tempFilePath))
      {
        var result = await resource.CopyNupkgToStreamAsync(identity.Id, version, stream, cache, _logger, cancellationToken);

        if (!result)
        {
          LogHelper.LogWarning($"Package {identity.Id} not downloaded");
          return null;
        }
      }
      
      var packageDirectoryPath = _fileService.Unzip(tempFilePath, _configService.LocalRepositoryPath);

      if (!_fileService.RemoveFile(tempFilePath)) 
        LogHelper.LogWarning($"Cannot remove temp file '{tempFilePath}'");

      return packageDirectoryPath;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private async Task FindPackageDependenciesRecursive(NuGet.Packaging.Core.PackageIdentity package,
      NuGetFramework framework,
      SourceCacheContext cacheContext,
      IEnumerable<SourceRepository> repositories,
      ISet<SourcePackageDependencyInfo> availablePackages,
      CancellationToken cancellationToken)
    {
      if (availablePackages.Contains(package)) 
        return;
      
      foreach (var repository in repositories)
      {
        var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
          package, framework, cacheContext, _logger, cancellationToken);

        if (dependencyInfo == null) 
          return;

        availablePackages.Add(dependencyInfo);
      
        foreach (var dependency in dependencyInfo.Dependencies)
        {
          await FindPackageDependenciesRecursive(
            new NuGet.Packaging.Core.PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
            framework, cacheContext, repositories, availablePackages, cancellationToken);
        }
      }
    }

    public NugetService(ILogger logger, 
      NugetConfigService configService, 
      FileService fileService, 
      FrameworkService frameworkService, 
      NugetPackageSource source)
    {
      _logger = logger;
      _configService = configService;
      _fileService = fileService;
      _frameworkService = frameworkService;
      Source = source;
    }
  }
}
  
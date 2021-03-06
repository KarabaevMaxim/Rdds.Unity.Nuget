using System;
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
using NuGet.Versioning;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.NugetConfig;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Utility;
using PackageIdentity = Rdds.Unity.Nuget.Entities.PackageIdentity;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.New.Services
{
  internal class NugetService : IDisposable
  {
    #region Fields and properties

    private readonly ILogger _logger;
    private readonly NugetConfigService _configService;
    private readonly FileService _fileService;
    private readonly FrameworkService _frameworkService;
    private readonly SourceCacheContext _cacheContext;

    public NugetPackageSource Source { get; }

    #endregion

    #region Public methods

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
      var repositories = new[] { Repository.Factory.GetCoreV3(Source.ToPackageSource()) };
      var currentFramework = _frameworkService.RequireCurrentFramework();
      var dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      
      await FindPackageDependenciesRecursive(
        packageIdentity.ToNugetPackageIdentity(),
        currentFramework.ToNugetFramework(),
        _cacheContext,
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
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(identity.Id, true, false, _cacheContext, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo()).FirstOrDefault(p => p.Identity.Version.Equals(identity.Version));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken)
    {
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      IEnumerable<NuGetVersion>? versions;

      try
      {
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        versions = await resource.GetAllVersionsAsync(packageId, _cacheContext, _logger, cancellationToken);
      }
      catch (TaskCanceledException)
      {
        return Enumerable.Empty<PackageVersion>();
      }
      catch(FatalProtocolException ex)
      {
        if (ex.InnerException?.GetType() != typeof(TaskCanceledException)) 
          LogHelper.LogWarningException(ex);
        
        return Enumerable.Empty<PackageVersion>();
      }
      
      versions = versions.OrderByDescending(v => v);
      var last = versions.First();
      return versions.Where(v => !v.IsPrerelease || v == last).Select(v => v.ToPackageVersion());
    }

    public async Task<string?> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var repository = Repository.Factory.GetCoreV3(Source.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var version = identity.Version.ToNugetVersion();
      var tempFilePath = _fileService.GetNupkgTempFilePath(identity);

      using (var stream = _fileService.CreateWriteFileStream(tempFilePath))
      {
        var result = await resource.CopyNupkgToStreamAsync(identity.Id, version, stream, _cacheContext, _logger, cancellationToken);

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

    #endregion

    #region IDisposable

    public void Dispose() => _cacheContext.Dispose();

    #endregion

    #region Private methods
    
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

    #endregion

    #region Constructor

    public NugetService(ILogger logger, 
      NugetConfigService configService, 
      FileService fileService, 
      FrameworkService frameworkService, 
      NugetPackageSource source,
      SourceCacheContext cacheContext)
    {
      _logger = logger;
      _configService = configService;
      _fileService = fileService;
      _frameworkService = frameworkService;
      _cacheContext = cacheContext;
      Source = source;
    }

    #endregion
  }
}
  
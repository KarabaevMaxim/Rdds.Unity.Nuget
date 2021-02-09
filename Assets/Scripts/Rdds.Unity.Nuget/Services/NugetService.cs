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
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;
using UnityEditor;
using PackageIdentity = Rdds.Unity.Nuget.Entities.PackageIdentity;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.Services
{
  public class NugetService : INugetService
  {
    private readonly ILogger _logger;
    private readonly NugetConfigService _configService;
    private readonly FileService _fileService;
    private readonly FrameworkService _frameworkService;

    private NugetPackageSource? _selectedSource;
    
    public NugetPackageSource SelectedSource
    {
      get
      {
        if (_selectedSource == null)
        {
          var key = EditorPrefs.GetString(nameof(SelectedSource), null);

          SelectedSource = string.IsNullOrWhiteSpace(key) 
            ? _configService.GetDefaultPackageSource() 
            : _configService.GetPackageSource(key);
        }

        return _selectedSource!;
      }
      private set
      {
        _selectedSource = value;
        EditorPrefs.SetString(nameof(SelectedSource), _selectedSource.Key);
      }
    }

    public void ChangeActiveSource(string key) => SelectedSource = _configService.GetPackageSource(key);

    public async Task<IEnumerable<PackageInfo>> SearchPackagesAsync(string filterString, int skip, int take, CancellationToken cancellationToken)
    {
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
      var searchFilter = new SearchFilter(true);
      var foundPackages =
        await resource.SearchAsync(filterString, searchFilter, skip, take, _logger, cancellationToken);

      return foundPackages.Select(p => p.ToPackageInfo());
    }

    [Obsolete("It doesn't always work")]
    public async Task<PackageInfo> FindDependenciesAsync(PackageInfo packageInfo,
      CancellationToken cancellationToken)
    {
      var cacheContext = new SourceCacheContext();
      var repositories = new[] { Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource()) };
      var currentFramework = _frameworkService.RequireCurrentFramework();
      var dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      
      await FindPackageDependenciesRecursive(
        packageInfo.Identity.ToNugetPackageIdentity(),
        currentFramework.ToNugetFramework(),
        cacheContext,
        repositories,
        dependencies,
        cancellationToken);

      packageInfo.Dependencies = new []
      {
        new FrameworkGroup(currentFramework, dependencies.Select(d => d.ToPackageIdentity()))
      };
      return packageInfo;
    }

    public async Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(identity.Id, true, false, cache, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo()).FirstOrDefault(p => p.Identity.Version.Equals(identity.Version));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var versions = await resource.GetAllVersionsAsync(packageId, cache, _logger, cancellationToken);
      versions = versions.OrderByDescending(v => v);
      var last = versions.First();
      return versions.Where(v => !v.IsPrerelease || v == last).Select(v => v.ToPackageVersion());
    }

    public async Task<bool> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var packageVersion = identity.Version.ToNugetVersion();
      var cacheFilePath = Config.GetCacheNupkgFileName(identity);

      using (var stream = _fileService.CreateWriteFileStream(cacheFilePath))
      {
        var result = await resource.CopyNupkgToStreamAsync(identity.Id, packageVersion, stream, cache, _logger, cancellationToken);

        if (!result)
        {
          _logger.LogWarning($"Package {identity.Id} not downloaded");
          return false;
        }
      }
      
      _fileService.Unzip(cacheFilePath, _configService.LocalRepositoryPath);
      return true;
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
    
    public NugetService(ILogger logger, NugetConfigService configService, FileService fileService, FrameworkService frameworkService)
    {
      _logger = logger;
      _configService = configService;
      _fileService = fileService;
      _frameworkService = frameworkService;
    }
  }
}
  
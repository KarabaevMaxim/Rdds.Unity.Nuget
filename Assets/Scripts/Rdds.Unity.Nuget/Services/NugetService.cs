using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;
using UnityEditor;
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

    public async Task<IEnumerable<PackageInfo>> GetPackagesAsync(string filterString, int skip, int take, CancellationToken cancellationToken)
    {
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageSearchResource>(cancellationToken);
      var searchFilter = new SearchFilter(true);
      var results =
        await resource.SearchAsync(filterString, searchFilter, skip, take, _logger, cancellationToken);

      return results.Select(r => r.ToPackageInfo());
      //
      // foreach (var res in result)
      // {
      //   var dep = await GetPackageDependencies(res.Identity); 
      // }
      //
      // return result;
    }

    public Task<SourcePackageDependencyInfo> GetPackageDependencies(PackageIdentity identity)
    {
      throw new NotImplementedException();
      // todo try it
      // var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken, );
      // var deps = await resource.GetDependencyInfoAsync();
      // var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      // var cache = new SourceCacheContext();
      // var resource = await repository.GetResourceAsync<DependencyInfoResource>();
      // return await resource.ResolvePackage(identity.ToNugetPackageIdentity(), new NuGetFramework(_frameworkService.GetFramework()), cache, _logger, CancellationToken.None);
    }

    public async Task<PackageInfo?> GetPackageAsync(string packageId, PackageVersion version, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(packageId, true, false, cache, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo()).FirstOrDefault(p => p.Identity.Version.Equals(version));
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
    
    public void InstallPackage(PackageInfo package)
    {
      
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
  
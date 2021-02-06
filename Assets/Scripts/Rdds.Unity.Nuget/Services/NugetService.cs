using System.Collections.Generic;
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
  public class NugetService
  {
    private readonly ILogger _logger;
    private readonly NugetConfigService _configService;
    private readonly FileService _fileService;

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
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
      var packages =
        await resource.GetMetadataAsync(packageId, true, false, cache, _logger, cancellationToken);
      return packages.Select(p => p.ToPackageInfo());
    }

    public async Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var versions = await resource.GetAllVersionsAsync(packageId, cache, _logger, cancellationToken);
      return versions.Select(v => v.ToPackageVersion());
    }

    public async Task<string?> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken)
    {
      var cache = new SourceCacheContext();
      var repository = Repository.Factory.GetCoreV3(SelectedSource.ToPackageSource());
      var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
      var packageVersion = identity.Version.ToNugetVersion();
      var cacheFilePath = Config.GetCacheNupkgFileName(identity);

      using var stream = _fileService.CreateWriteFileStream(cacheFilePath);
      var result = await resource.CopyNupkgToStreamAsync(identity.Id, packageVersion, stream, cache, _logger, cancellationToken);
        
      if (!result)
        _logger.LogWarning($"Package {identity.Id} not downloaded");
      
      return cacheFilePath;
    }
    
    public void InstallPackage(PackageInfo package)
    {
      
    }

    public NugetService(ILogger logger, NugetConfigService configService, FileService fileService)
    {
      _logger = logger;
      _configService = configService;
      _fileService = fileService;
    }
  }
}
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.NugetConfig;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.New.Services
{
  internal class RemotePackagesService
  {
    #region Fields and properties

    private readonly NugetConfigService _nugetConfigService;
    private readonly FileService _fileService;
    private readonly FrameworkService _frameworkService;
    private readonly ILogger _logger;
    private Dictionary<string, NugetService> _nugetServices = null!;

    private NugetPackageSource? _selectedSource;

    public NugetPackageSource? SelectedSource
    {
      get
      {
        if (_selectedSource == null)
        {
          var key = EditorPrefs.GetString(nameof(SelectedSource), null);

          SelectedSource = string.IsNullOrWhiteSpace(key) 
            ? null
            : _nugetConfigService.RequirePackageSource(key);
        }

        return _selectedSource!;
      }
      private set
      {
        _selectedSource = value;
        EditorPrefs.SetString(nameof(SelectedSource), _selectedSource?.Key);
      }
    }
    
    #endregion

    public void ChangeActiveSource(string? key) => 
      SelectedSource = string.IsNullOrWhiteSpace(key) ? null : _nugetConfigService.RequirePackageSource(key!);

    public async Task<IEnumerable<PackageInfoSourceWrapper>> FindPackagesAsync(string filterString, int skip, int take, CancellationToken cancellationToken)
    {
      if (SelectedSource == null)
      {
        var tasks = _nugetServices
          .Select(p => p.Value)
          .Select(async service => (await service.SearchPackagesAsync(filterString, skip, take, cancellationToken))
            .Select(package => new PackageInfoSourceWrapper(package, new []{ service.Source.Key })));
        var allPackages = await Task.WhenAll(tasks);
        return allPackages.SelectMany(p => p);
      }

      var packages = await _nugetServices[SelectedSource.Key]
        .SearchPackagesAsync(filterString, skip, take, cancellationToken);
      return packages.Select(p => new PackageInfoSourceWrapper(p, new []{ SelectedSource.Key }));
    }

    public IEnumerable<PackageVersion> FindPackageVersions(string packageId)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<FrameworkGroup> FindDependencies(PackageIdentity packageIdentity)
    {
      throw new NotImplementedException();
    }
    
    public string DownloadPackage(PackageIdentity identity)
    { 
      // загружает архив из сети
      // распаковывает его в репозиторий
      // LocalPackagesConfigService.AddLocalPackage(identity, localPath)
      throw new NotImplementedException();
    }

    public void InitializeSources()
    {
      var nugetServices = _nugetConfigService.RequireAvailableSources()
        .Select(source => new NugetService(_logger, _nugetConfigService, _fileService, _frameworkService, source));
      _nugetServices = nugetServices.ToDictionary(s => s.Source.Key);
    }

    #region Constructor

    public RemotePackagesService(NugetConfigService nugetConfigService, 
      FileService fileService, 
      FrameworkService frameworkService, 
      ILogger logger)
    {
      _nugetConfigService = nugetConfigService;
      _fileService = fileService;
      _frameworkService = frameworkService;
      _logger = logger;
    }

    #endregion
  }
}
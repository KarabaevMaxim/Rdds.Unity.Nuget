using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.Services
{
  public class InstalledPackagesService
  {
    private readonly PackagesFileService _packagesFileService;
    private readonly INugetService _nugetService;
    private readonly NugetConfigService _nugetConfigService;
    private readonly ILogger _logger;

    public async Task<IEnumerable<PackageInfo>> UpdateInstalledPackagesListAsync(CancellationToken cancellationToken)
    {
      var installedPackages = _packagesFileService.RequirePackages();

      var tasks = installedPackages.Select(pi =>
      {
        var source = _nugetConfigService.RequirePackageSource(pi.Item2);
        var result = _nugetService.GetPackageAsync(pi.Item1, source, cancellationToken);
        _logger.LogWarning($"Package {pi.Item1.Id} with version {pi.Item1.Version} not found in source {source.Key}");
        return result;
      });

      var packages = (await Task.WhenAll(tasks)).Where(p => p != null);
      return packages!;
    }

    public InstalledPackagesService(PackagesFileService packagesFileService, INugetService nugetService,
      NugetConfigService nugetConfigService, ILogger logger)
    {
      _packagesFileService = packagesFileService;
      _nugetService = nugetService;
      _nugetConfigService = nugetConfigService;
      _logger = logger;
    }
  }
}
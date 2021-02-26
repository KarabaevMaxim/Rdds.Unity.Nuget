using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Exceptions;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  [Obsolete]
  internal class InstalledPackagesService
  {
    private readonly PackagesFileService _packagesFileService;
    private readonly NugetService _nugetService;
    private readonly NugetConfigService _nugetConfigService;
    private readonly NuspecFileService _nuspecFileService;

    public async Task<IEnumerable<PackageInfo>> UpdateInstalledPackagesListAsync(CancellationToken cancellationToken)
    {
      var installedPackages = _packagesFileService.RequirePackages();

      var tasks = installedPackages.Select(async pi =>
      {
        var source = _nugetConfigService.RequirePackageSource(pi.Item2);
        var result = await _nugetService.GetPackageAsync(pi.Item1, cancellationToken);

        if (result == null)
          LogHelper.LogWarning($"Package {pi.Item1.Id} with version {pi.Item1.Version} not found in source {source.Key}");

        return result;
      });

      var packages = (await Task.WhenAll(tasks)).Where(p => p != null);
      return packages!;
    }

    public IEnumerable<Task<PackageInfo>> RequireInstalledPackagesList()
    {
      return _packagesFileService.RequirePackageModels()
        .Select(p => _nuspecFileService.RequirePackageInfoFromNuspecAsync(p.Path));
    }

    /// <summary>
    /// Shows that dlls from nuget package locating in Plugins directory.
    /// </summary>
    public bool IsPackageDllInProject(string packageId) => true;
    
    public bool IsPackageInstalled(string packageId) => _packagesFileService.HasPackage(packageId) && IsPackageDllInProject(packageId);

    public PackageVersion RequireVersionInstalledPackage(string packageId)
    {
      if (!IsPackageInstalled(packageId))
        throw new PackageNotInstalledException(packageId);

      var package = _packagesFileService.RequirePackage(packageId);
      return PackageVersion.Parse(package.Version);
    }

    public bool EqualInstalledPackageVersion(PackageIdentity identity)
    {
      if (!IsPackageInstalled(identity.Id))
        throw new PackageNotInstalledException(identity.Id);

      var package = _packagesFileService.RequirePackage(identity.Id);
      return identity.Version.Equals(PackageVersion.Parse(package.Version));
    }

    public async Task<bool> InstallPackageAsync(string packageDirectoryPath, string sourceKey, string assemblyName)
    {
      var packageInfo = await _nuspecFileService.GetPackageInfoFromNuspecAsync(packageDirectoryPath);

      if (packageInfo == null)
      {
        LogHelper.LogWarning($"Error occurred while reading .nuspec file of package {packageDirectoryPath}");
        return false;
      }
      
      // todo add dlls in Plugins, add reference to dll in asmdef
      _packagesFileService.AddOrUpdatePackage(packageInfo.Identity, sourceKey, packageDirectoryPath);
      _packagesFileService.InstallPackageInAssembly(packageInfo.Identity.Id, assemblyName);
      await _packagesFileService.SavePackagesFileAsync();
      return true;
    }
    
    public async Task<bool> RemovePackageAsync(PackageIdentity identity, string assemblyName)
    {
      // todo remove dlls from Plugins
      try
      {
        _packagesFileService.RemovePackageFromAssembly(identity.Id, assemblyName);
        _packagesFileService.RemovePackage(identity);
      }
      catch (PackageNotInstalledException ex)
      {
        LogHelper.LogWarningException(ex);
        return false;
      }

      await _packagesFileService.SavePackagesFileAsync();
      return true;
    }
    
    public InstalledPackagesService(PackagesFileService packagesFileService, NugetService nugetService,
      NugetConfigService nugetConfigService, NuspecFileService nuspecFileService)
    {
      _packagesFileService = packagesFileService;
      _nugetService = nugetService;
      _nugetConfigService = nugetConfigService;
      _nuspecFileService = nuspecFileService;
    }
  }
}
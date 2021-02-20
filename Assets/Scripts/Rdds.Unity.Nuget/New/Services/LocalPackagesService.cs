using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.New.Services
{
  internal class LocalPackagesService
  {
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;
    private readonly NuspecFileService _nuspecFileService;
    private readonly DllFileService _dllFileService;
    private readonly AssembliesService _assembliesService;

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<bool> InstallPackage(PackageIdentity identity, IEnumerable<string> assembliesToInstall, Framework targetFramework)
    {
      var path = _localPackagesConfigService.GetPackagePath(identity);

      if (path == null)
      {
        LogHelper.LogWarning($"Package{identity.Id} {identity.Version} not downloaded");
        return false;
      }


      var dlls = _dllFileService.CopyDlls(identity, targetFramework);
      _dllFileService.ConfigureDlls(dlls);
      var changedAssemblies= await _assembliesService.AddDllReferencesAsync(assembliesToInstall, dlls);
      _installedPackagesConfigService.AddInstalledPackage(identity, changedAssemblies.ToList(), dlls);
      await _installedPackagesConfigService.SaveConfigFileAsync();
      return true;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<bool> RemovePackage(string packageId, List<string> assemblies)
    {
      var dllNames = _installedPackagesConfigService.RequireDllNames(packageId);
      await _assembliesService.RemoveDllReferencesAsync(assemblies, dllNames);
      var needRemoveDlls = _installedPackagesConfigService.RemoveInstalledPackage(packageId, assemblies);
      await _installedPackagesConfigService.SaveConfigFileAsync();
      var result = true;
      
      if (needRemoveDlls) 
        result &= _dllFileService.RemoveDlls(dllNames);

      return result;
    }

    public IEnumerable<PackageIdentity> RequireInstalledPackages() => 
      _installedPackagesConfigService.InstalledPackages.Select(p => new PackageIdentity(p.Id, p.Version));

    public PackageInfo RequireInstalledPackageInfo(string packageId)
    {
      var package = _installedPackagesConfigService.RequireInstalledPackage(packageId);
      var path = _localPackagesConfigService.RequirePackagePath(new PackageIdentity(package.Id, package.Version));
      return _nuspecFileService.RequirePackageInfoFromNuspec(path);
    }

    public LocalPackagesService(InstalledPackagesConfigService installedPackagesConfigService,
      LocalPackagesConfigService localPackagesConfigService,
      NuspecFileService nuspecFileService,
      DllFileService dllFileService,
      AssembliesService assembliesService)
    {
      _installedPackagesConfigService = installedPackagesConfigService;
      _localPackagesConfigService = localPackagesConfigService;
      _nuspecFileService = nuspecFileService;
      _dllFileService = dllFileService;
      _assembliesService = assembliesService;
    }
  }
}
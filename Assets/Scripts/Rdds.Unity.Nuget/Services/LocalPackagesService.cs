using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services.Configs;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  internal class LocalPackagesService
  {
    #region Fields

    private readonly InstalledPackagesConfigService _installedPackagesConfigService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;
    private readonly NuspecFileService _nuspecFileService;
    private readonly DllFilesService _dllFilesService;
    private readonly AssembliesService _assembliesService;

    #endregion

    #region Public methods

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<bool> InstallPackageAsync(PackageIdentity identity, IEnumerable<string> assembliesToInstall, Framework targetFramework)
    {
      var allInstallingPackagesContainer = new HashSet<PackageIdentity>();
      var successInstalledPackagesContainer = new HashSet<PackageIdentity>();
      await InstallPackageInternalAsync(identity, assembliesToInstall, targetFramework, 
        allInstallingPackagesContainer, successInstalledPackagesContainer).ConfigureAwait(false);
      await _installedPackagesConfigService.SaveConfigFileAsync().ConfigureAwait(false);
      
      if (allInstallingPackagesContainer.Count != successInstalledPackagesContainer.Count)
      {
        LogHelper.LogWarning("Not all packages from the dependency tree are installed. Rolling back...");

        foreach (var installed in successInstalledPackagesContainer)
          await RemovePackageAsync(installed.Id, assembliesToInstall);

        return false;
      }
      
      return true;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<bool> RemovePackageAsync(string packageId, IEnumerable<string> assemblies)
    {
      var dllNames = _installedPackagesConfigService.RequireDllNames(packageId);
      await _assembliesService.RemoveDllReferencesAsync(assemblies, dllNames);
      var needRemoveDlls = _installedPackagesConfigService.RemoveInstalledPackage(packageId, assemblies);
      await _installedPackagesConfigService.SaveConfigFileAsync();

      if (needRemoveDlls) 
        return _dllFilesService.RemoveDlls(dllNames);

      return true;
    }

    public IEnumerable<PackageIdentity> RequireInstalledPackages() => 
      _installedPackagesConfigService.InstalledPackages.Select(p => new PackageIdentity(p.Id, p.Version));
    
    public Task<PackageInfo> RequireInstalledPackageInfoAsync(string packageId)
    {
      var package = _installedPackagesConfigService.RequireInstalledPackage(packageId);
      var path = _localPackagesConfigService.RequirePackagePath(new PackageIdentity(package.Id, package.Version));
      return _nuspecFileService.RequirePackageInfoFromNuspecAsync(path);
    }
    
    public Task<PackageInfo?> GetInstalledPackageInfoAsync(string packageId)
    {
      var package = _installedPackagesConfigService.RequireInstalledPackage(packageId);
      var path = _localPackagesConfigService.RequirePackagePath(new PackageIdentity(package.Id, package.Version));
      return _nuspecFileService.GetPackageInfoFromNuspecAsync(path);
    }

    public bool IsPackageInstalled(string packageId)
    {
      var package = _installedPackagesConfigService.GetInstalledPackage(packageId);
      return package != null;
    }

    #endregion

    #region Private methods

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private async Task InstallPackageInternalAsync(
      PackageIdentity identity, 
      IEnumerable<string> assembliesToInstall, 
      Framework targetFramework,
      ISet<PackageIdentity> installingPackagesContainer,
      ISet<PackageIdentity> successInstalledPackagesContainer)
    {
      if (!installingPackagesContainer.Contains(identity)) 
        installingPackagesContainer.Add(identity);

      if (IsPackageInstalled(identity.Id))
      {
        successInstalledPackagesContainer.Add(identity);
        return;
      }

      var info = await RequireLocalPackageInfo(identity);
      var dependenciesGroup = info.Dependencies?.FirstOrDefault(d=> d.TargetFramework.Equals(targetFramework));
      
      if (dependenciesGroup != null)
      {
        foreach (var dependencyIdentity in dependenciesGroup.Dependencies)
        {
          await InstallPackageInternalAsync(dependencyIdentity, assembliesToInstall, targetFramework, 
              installingPackagesContainer, 
              successInstalledPackagesContainer);
        }
      }
      
      var path = _localPackagesConfigService.GetPackagePath(identity);

      if (path == null)
      {
        LogHelper.LogWarning($"Package {identity.Id} {identity.Version} not downloaded");
        return;
      }
      
      var dlls = _dllFilesService.CopyDlls(identity, targetFramework);
      _dllFilesService.ConfigureDlls(dlls);
      var dllNames = dlls.Select(Path.GetFileName);
      var changedAssemblies = await _assembliesService.AddDllReferencesAsync(assembliesToInstall, dllNames);
      _installedPackagesConfigService.AddInstalledPackageOrUpdate(identity, changedAssemblies.ToList(), dllNames.ToList());
      successInstalledPackagesContainer.Add(identity);
    }

    private Task<PackageInfo> RequireLocalPackageInfo(PackageIdentity identity)
    {
      var path = _localPackagesConfigService.RequirePackagePath(identity);
      return _nuspecFileService.RequirePackageInfoFromNuspecAsync(path);
    }
    
    #endregion
    
    #region Constructor

    public LocalPackagesService(InstalledPackagesConfigService installedPackagesConfigService,
      LocalPackagesConfigService localPackagesConfigService,
      NuspecFileService nuspecFileService,
      DllFilesService dllFilesService,
      AssembliesService assembliesService)
    {
      _installedPackagesConfigService = installedPackagesConfigService;
      _localPackagesConfigService = localPackagesConfigService;
      _nuspecFileService = nuspecFileService;
      _dllFilesService = dllFilesService;
      _assembliesService = assembliesService;
    }

    #endregion
  }
}
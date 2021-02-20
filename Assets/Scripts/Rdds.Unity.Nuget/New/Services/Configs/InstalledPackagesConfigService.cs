using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Exceptions;
using Rdds.Unity.Nuget.New.Services.Configs.Models;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.New.Services.Configs
{
  internal class InstalledPackagesConfigService
  {
    private const string ConfigName = "InstalledPackages.json";
    
    private readonly FileService _fileService;
    
    private List<InstalledPackageInfo> _installedPackages = new List<InstalledPackageInfo>();

    public IReadOnlyList<InstalledPackageInfo> InstalledPackages => _installedPackages;

    public async Task LoadConfigFileAsync()
    {
      var json = await _fileService.ReadFromFileAsync(ConfigName, CancellationToken.None);

      if (json == null)
      {
        LogHelper.LogWarning($"{ConfigName} file not found, a new file will be created");
        await SaveDefaultConfigFileAsync();
        return;
      }
      
      try
      {
        _installedPackages = JsonConvert.DeserializeObject<List<InstalledPackageInfo>>(json);
      }
      catch (JsonException ex)
      {
        LogHelper.LogWarningException($"An error occurred while reading {ConfigName} file, a new file will be created", ex);
        await SaveDefaultConfigFileAsync();
      }
    }

    public async Task SaveConfigFileAsync()
    {
      var json = JsonConvert.SerializeObject(_installedPackages);
      await _fileService.WriteToFileAsync(ConfigName, json);
    }
    
    public void AddInstalledPackage(PackageIdentity identity, List<string> installedInAssemblies, IEnumerable<string> dllNames) => 
      _installedPackages.Add(new InstalledPackageInfo(identity.Id, identity.Version.ToString(), installedInAssemblies, dllNames));

    public void AddPackageToAssemblies(string packageId, IEnumerable<string> installedInAssemblies)
    {
      var package = RequireInstalledPackage(packageId);
      package.InstalledInAssemblies.AddRange(installedInAssemblies);
    }
    
    public bool RemoveInstalledPackage(string packageId, IEnumerable<string> assemblies)
    {
      var package = RequireInstalledPackage(packageId);
      package.InstalledInAssemblies.RemoveAll(assemblies.Contains);

      if (package.InstalledInAssemblies.Count == 0)
      {
        _installedPackages.Remove(package);
        return true;
      }

      return false;
    }

    public IEnumerable<string> RequireDllNames(string packageId) => RequireInstalledPackage(packageId).DllNames;

    public InstalledPackageInfo RequireInstalledPackage(string packageId)
    {
      var package = GetInstalledPackage(packageId);

      if (package == null)
        throw new PackageNotInstalledException(packageId);

      return package;
    }

    public InstalledPackageInfo? GetInstalledPackage(string packageId) => 
      _installedPackages.FirstOrDefault(p => p.Id == packageId);
    
    
    private async Task SaveDefaultConfigFileAsync()
    {
      _installedPackages = new List<InstalledPackageInfo>();
      await SaveConfigFileAsync();
    }

    public InstalledPackagesConfigService(FileService fileService) => _fileService = fileService;
  }
}
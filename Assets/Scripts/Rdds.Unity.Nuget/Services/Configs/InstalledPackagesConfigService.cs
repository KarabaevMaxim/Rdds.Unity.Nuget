﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Exceptions;
using Rdds.Unity.Nuget.Services.Configs.Models;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services.Configs
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
        var backupPath = await _fileService.BackupFileAsync(ConfigName, CancellationToken.None);
        LogHelper.LogWarningException($"An error occurred while reading {ConfigName} file, a new file will be created. Copy of old file placed at {backupPath}", ex);
        await SaveDefaultConfigFileAsync();
      }
    }

    public async Task SaveConfigFileAsync()
    {
      var json = JsonConvert.SerializeObject(_installedPackages, Formatting.Indented);
      await _fileService.WriteToFileAsync(ConfigName, json);
    }
    
    public void AddInstalledPackage(PackageIdentity identity, List<string> installedInAssemblies, List<string> dllNames)
    {
      var foundPackage = GetInstalledPackage(identity.Id);
      
      if (foundPackage != null)
        return;

      _installedPackages.Add(
        new InstalledPackageInfo(identity.Id, identity.Version.ToString(), installedInAssemblies, dllNames));
    }

    public void AddInstalledPackageOrUpdate(PackageIdentity identity, List<string> installedInAssemblies, List<string> dllNames)
    {
      var foundPackage = GetInstalledPackage(identity.Id);

      if (foundPackage != null)
      {
        foundPackage.InstalledInAssemblies.AddRange(installedInAssemblies.Except(foundPackage.InstalledInAssemblies));
        foundPackage.DllNames.AddRange(dllNames.Except(foundPackage.DllNames));
        return;
      }
      
      _installedPackages.Add(
        new InstalledPackageInfo(identity.Id, identity.Version.ToString(), installedInAssemblies, dllNames));
    }

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

    public IEnumerable<string> RequireInstalledInAssemblies(string packageId) => 
      RequireInstalledPackage(packageId).InstalledInAssemblies;
    
    public IEnumerable<string>? GetInstalledInAssemblies(string packageId) => 
      GetInstalledPackage(packageId)?.InstalledInAssemblies;

    private async Task SaveDefaultConfigFileAsync()
    {
      // _installedPackages = new List<InstalledPackageInfo>();
      _installedPackages = new List<InstalledPackageInfo>
      {
        new InstalledPackageInfo("Newtonsoft.Json", "8.0.2", new List<string> { "Rdds.Unity.Nuget" }, new List<string> { "Newtonsoft.Json.dll" } )
      };
      await SaveConfigFileAsync();
    }

    public InstalledPackagesConfigService(FileService fileService) => _fileService = fileService;
  }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.PackagesFile;
using Rdds.Unity.Nuget.New.Exceptions;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  [Obsolete]
  internal class PackagesFileService
  {
    private readonly FileService _fileService;

    private readonly string _packagesFileName = Path.Combine("NugetPackages.json");

    private Packages _packages = null!;

    public async Task SavePackagesFileAsync()
    {
      var json = JsonConvert.SerializeObject(_packages, Formatting.Indented);
      await _fileService.WriteToFileAsync(_packagesFileName, json);
    }
    
    public async Task LoadConfigFileAsync()
    {
      var json = await _fileService.ReadFromFileAsync(_packagesFileName, CancellationToken.None);

      if (string.IsNullOrWhiteSpace(json))
      {
        LogHelper.LogWarning("Packages file not found. Will be create a new one.");
        await CreateDefaultPackagesFileAsync();
        return;
      }
      
      try
      {
        _packages = JsonConvert.DeserializeObject<Packages>(json!);
      }
      catch (Exception ex)
      {
        LogHelper.LogWarningException("Error occurred while reading packages file. Will be create a new one", ex);
        await CreateDefaultPackagesFileAsync();
      }

      ValidatePackagesFile();
    }

    public async Task CreateDefaultPackagesFileAsync()
    {
      _packages = new Packages(new List<Package>());
      await SavePackagesFileAsync();
    }

    public void InstallPackageInAssembly(string packageId, string assemblyName)
    {
      var foundPackage = RequirePackage(packageId);
      
      if (foundPackage.InstalledInAssemblies.Contains(assemblyName))
        // todo throw PackageAlreadyInstalled exception?
        return;

      foundPackage.InstalledInAssemblies.Add(assemblyName);
    }
    
    public void RemovePackageFromAssembly(string packageId, string assemblyName)
    {
      var foundPackage = RequirePackage(packageId);
      
      if (!foundPackage.InstalledInAssemblies.Contains(assemblyName))
        // todo throw PackageNotInstalled exception?
        return;

      foundPackage.InstalledInAssemblies.Remove(assemblyName);
    }
    
    public void AddOrUpdatePackage(PackageIdentity identity, string sourceKey, string packageDirectoryPath)
    {
      if (!Directory.Exists(packageDirectoryPath))
        throw new ArgumentOutOfRangeException(nameof(packageDirectoryPath), "Directory not found");
      
      var foundPackage = GetPackage(identity.Id);

      if (foundPackage != null) 
         RemovePackage(foundPackage);

      _packages.PackagesList.Add(new Package(identity.Id, identity.Version.ToString(), sourceKey, packageDirectoryPath,
        foundPackage?.InstalledInAssemblies ?? new List<string>()));
    }

    public void RemovePackage(PackageIdentity identity)
    {
      var foundPackage = RequirePackage(identity.Id);
      RemovePackage(foundPackage);
    }

    public IEnumerable<(PackageIdentity, string)> RequirePackages() => 
      _packages.PackagesList.Select(p => (new PackageIdentity(p.Id, PackageVersion.Parse(p.Version)), p.Source));

    public IEnumerable<Package> RequirePackageModels() => _packages.PackagesList;

    public bool HasPackage(string packageId) => GetPackage(packageId) != null;

    public Package? GetPackage(string packageId) => _packages.PackagesList.FirstOrDefault(p => p.Id == packageId);
    
    public Package RequirePackage(string packageId)
    {
      var foundPackage = GetPackage(packageId);

      if (foundPackage == null)
        throw new PackageNotInstalledException(packageId);

      return foundPackage;
    }

    private bool RemovePackage(Package package) => _packages.PackagesList.Remove(package);

    private void ValidatePackagesFile()
    {
      var issues = new StringBuilder();
      
      foreach (var package in _packages.PackagesList)
      {
        if (!PackageVersion.TryParse(package.Version, out _))
          issues.AppendLine($"  -Format of version of package {package.Id} is not valid");

        if (!Directory.Exists(package.Path))
          issues.AppendLine($"  -Root directory {package.Path} of package {package.Id} not found");
      }
      
      if (issues.Length == 0)
        return;
      
      issues.Insert(0, $"Found issues with {_packagesFileName} file:{Environment.NewLine}");
      LogHelper.LogWarning(issues.ToString());
    }

    public PackagesFileService(FileService fileService) => _fileService = fileService;
  }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Exceptions;
using Rdds.Unity.Nuget.Services.Configs;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  internal class AssembliesService
  {
    private const string CodeRootDirectory = "Assets/Scripts";
    
    private readonly FileService _fileService;
    private readonly InstalledPackagesConfigService _installedPackagesConfigService;

    public async Task<IEnumerable<AssemblyModel>> RequireAllAssembliesAsync()
    {
      var tasks = _fileService.FindFiles(CodeRootDirectory, "asmdef", true).Select(async filePath =>
      {
        try
        {
          return await ParseAssemblyFileAsync(filePath);
        }
        catch (JsonException ex)
        {
          LogHelper.LogWarningException($"Failed parse .asmdef file {filePath}", ex);
          return null;
        }
      });

      var currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
      return (await Task.WhenAll(tasks)).Where(a => a != null && a.Name != currentAssemblyName)!;
    }

    public async Task<IEnumerable<AssemblyModel>> RequireAssembliesAsync(IEnumerable<string> assemblyNames)
    {
      var assemblies = await RequireAllAssembliesAsync();
      return assemblies.Where(a => assemblyNames.Contains(a.Name));
    }
    
    public async Task<AssemblyModel> RequireAssemblyAsync(string assemblyName)
    {
      var assemblies = await RequireAllAssembliesAsync();
      return assemblies.First(a => a.Name == assemblyName);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IEnumerable<string>> AddDllReferencesAsync(IEnumerable<string> assemblyNames, IEnumerable<string> dllPaths)
    {
      var assemblies = await RequireAssembliesAsync(assemblyNames);
      var changedAssemblies = new List<string>();

      foreach (var assembly in assemblies)
      {
        if (await AddDllReferencesInternalAsync(assembly, dllPaths)) 
          changedAssemblies.Add(assembly.Name);
      }

      return changedAssemblies;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<bool> RemoveDllReferencesAsync(IEnumerable<string> assemblyNames, IEnumerable<string> dllNames)
    {
      var assemblies = await RequireAssembliesAsync(assemblyNames);
      var success = true;
      
      foreach (var assembly in assemblies) 
        success &= await RemoveDllReferencesInternalAsync(assembly, dllNames);

      return success;
    }

    public bool IsPackageInstalledInAssembly(string packageId, string assemblyName)
    {
      var package = _installedPackagesConfigService.GetInstalledInAssemblies(packageId);
      return package != null && package.Any(a => a == assemblyName);
    }
    
    public string RequireInstalledPackageVersion(string packageId, string assemblyName)
    {
      var package = _installedPackagesConfigService.RequireInstalledPackage(packageId);

      if (package.InstalledInAssemblies.All(a => a != assemblyName))
        throw new PackageNotInstalledException(packageId);

      return package.Version;
    }
     
    private async Task<bool> AddDllReferencesInternalAsync(AssemblyModel assembly, IEnumerable<string> dllPaths)
    {
      if (!assembly.OverrideReferences)
        return true;

      foreach (var dll in dllPaths)
      {
        var dllName = Path.GetFileName(dll);
        
        if (assembly.PrecompiledReferences.Contains(dllName))
        {
          LogHelper.LogWarning($"Dll {dllName} already installed in assembly {assembly.Name}");
          continue;
        }
        
        assembly.PrecompiledReferences.Add(dllName);
      }
      
      return await SaveAssemblyAsync(assembly);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private async Task<bool> RemoveDllReferencesInternalAsync(AssemblyModel assembly, IEnumerable<string> dllNames)
    {
      if (!assembly.OverrideReferences)
        return true;

      var removedCount = dllNames.Count(dllName => assembly.PrecompiledReferences.Remove(dllName));
      var saveResult = await SaveAssemblyAsync(assembly);
      return removedCount == dllNames.Count() && saveResult;
    }
    
    private async Task<bool> SaveAssemblyAsync(AssemblyModel assembly)
    {
      string json;
      var backupFilePath = string.Empty;
      
      try
      {
        backupFilePath = _fileService.CopyFile(assembly.Path,
          Path.Combine(_fileService.TempDirectoryPath, "Backup"));
      }
      catch (Exception ex)
      {
        LogHelper.LogWarningException($"Failed to create backup of file '{assembly.Path}'", ex);
      }
      
      try
      {
        json = assembly.Properties.ToString();
      }
      catch (JsonException ex)
      {
        LogHelper.LogWarningException($"Failed serialize assembly object {assembly.Name}", ex);
        return false;
      }

      try
      {
        await _fileService.WriteToFileAsync(assembly.Path, json);
      }
      catch (IOException ex)
      {
        LogHelper.LogWarningException( $"Failed save assembly file {assembly.Path}. Backup: {backupFilePath}", ex);
        return false;
      }

      if (!string.IsNullOrWhiteSpace(backupFilePath))
        _fileService.RemoveFile(backupFilePath!);

      return true;
    }

    private async Task<AssemblyModel> ParseAssemblyFileAsync(string filePath)
    {
      var json = await _fileService.RequireFileContentAsync(filePath, CancellationToken.None);
      var result = new AssemblyModel(JObject.Parse(json)) { Path = filePath };
      return result;
    }

    public AssembliesService(FileService fileService, InstalledPackagesConfigService installedPackagesConfigService)
    {
      _fileService = fileService;
      _installedPackagesConfigService = installedPackagesConfigService;
    }
  }
}
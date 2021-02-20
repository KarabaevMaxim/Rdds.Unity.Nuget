using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  internal class AssembliesService
  {
    private const string CodeRootDirectory = "Assets/Scripts";
    
    private readonly FileService _fileService;

    public async Task<IEnumerable<AssemblyModel>> RequireAllAssembliesAsync()
    {
      var tasks = _fileService.FindFiles(CodeRootDirectory, "asmdef", true).Select(async f =>
      {
        var json = await _fileService.RequireFileContentAsync(f, CancellationToken.None);

        try
        {
          return JsonConvert.DeserializeObject<AssemblyModel>(json);
        }
        catch (JsonException ex)
        {
          LogHelper.LogWarningException($"Failed parse asmdef file {f}", ex);
          return null;
        }
      });

      return (await Task.WhenAll(tasks)).Where(a => a != null)!;
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
        {
          changedAssemblies.Add(assembly.Path);
        }
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
     
    private async Task<bool> AddDllReferencesInternalAsync(AssemblyModel assembly, IEnumerable<string> dllPaths)
    {
      if (!assembly.OverrideReferences)
        return true;
      
      assembly.PrecompiledReferences ??= new List<string>();

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
      
      var removedCount = assembly.PrecompiledReferences?.RemoveAll(dllNames.Contains);
      var saveResult = await SaveAssemblyAsync(assembly);
      return removedCount.GetValueOrDefault(-1) == dllNames.Count() && saveResult;
    }
    
    private async Task<bool> SaveAssemblyAsync(AssemblyModel assembly)
    {
      string json;
      
      try
      {
        json = JsonConvert.SerializeObject(assembly);
      }
      catch (JsonException ex)
      {
        LogHelper.LogWarningException($"Failed serialize assembly object {assembly.Name}", ex);
        return false;
      }

      var backupFilePath = _fileService.CopyFile(assembly.Path, _fileService.TempDirectoryPath);;

      try
      {
        await _fileService.WriteToFileAsync(assembly.Path, json);
      }
      catch (IOException ex)
      {
        LogHelper.LogWarningException( $"Failed save assembly file {assembly.Path}. Backup: {backupFilePath}", ex);
        return false;
      }

      return true;
    }

    public AssembliesService(FileService fileService) => _fileService = fileService;
  }
}
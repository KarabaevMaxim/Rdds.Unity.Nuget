using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.New.Services.Configs;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;

namespace Rdds.Unity.Nuget.New.Services
{
  internal class DllFileService
  {
    private readonly string _dllsDirectory = Path.Combine("Assets", "Plugins");
    private readonly FileService _fileService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;

    public IEnumerable<string> CopyDlls(PackageIdentity identity, Framework targetFramework)
    {
      var packagePath = _localPackagesConfigService.RequirePackagePath(identity);
      var frameworkDirectories = Directory.GetDirectories(Path.Combine(packagePath, "lib"));
      var targetDirectory = frameworkDirectories.First(d => d == targetFramework.Name);
      return _fileService.CopyFilesFromDirectory(targetDirectory, "dll", _dllsDirectory);
    }

    public void ConfigureDlls(IEnumerable<string> dllPaths)
    {
      foreach (var dll in dllPaths)
      {
        var importer = AssetImporter.GetAtPath(dll);
        ReflectionHelper.SetPrivateProperty(importer, "ValidateReferences", false);
      }
    }

    public bool RemoveDlls(IEnumerable<string> dllNames)
    {
      var result = true;
      
      foreach (var dllName in dllNames) 
        result &= _fileService.RemoveFile(Path.Combine(_dllsDirectory, dllName));

      return result;
    }

    public DllFileService(FileService fileService, LocalPackagesConfigService localPackagesConfigService)
    {
      _fileService = fileService;
      _localPackagesConfigService = localPackagesConfigService;
    }
  }
}
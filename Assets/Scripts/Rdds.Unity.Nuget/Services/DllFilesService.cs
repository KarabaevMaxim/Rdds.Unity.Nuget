using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services.Configs;
using Rdds.Unity.Nuget.Utility;
using UnityEditor;

namespace Rdds.Unity.Nuget.Services
{
  internal class DllFilesService
  {
    private readonly string _dllsDirectory = Path.Combine("Assets", "Plugins");
    private readonly FileService _fileService;
    private readonly LocalPackagesConfigService _localPackagesConfigService;

    public IEnumerable<string> CopyDlls(PackageIdentity identity, Framework targetFramework)
    {
      var packagePath = _localPackagesConfigService.RequirePackagePath(identity);
      var frameworkDirectories = Directory.GetDirectories(Path.Combine(packagePath, "lib"));
      var targetDirectory = frameworkDirectories.First(d => Path.GetFileName(d) == targetFramework.Name);
      var newAssets = _fileService.ImportAssetsFromDirectory(targetDirectory, "dll", _dllsDirectory);
      return newAssets;
    }

    public void ConfigureDlls(IEnumerable<string> dllPaths)
    {
      ThreadHelper.RunInMainThread(() =>
      {
        foreach (var dll in dllPaths)
        {
          var importer = AssetImporter.GetAtPath(dll);
          ReflectionHelper.SetNonPublicProperty(importer, "ValidateReferences", false);
        } 
      });
    }

    public bool RemoveDlls(IEnumerable<string> dllNames)
    {
      var failedPaths = new List<string>();
      ThreadHelper.RunInMainThread(() => 
        AssetDatabase.DeleteAssets(dllNames.Select(n => Path.Combine(_dllsDirectory, n)).ToArray(), failedPaths));

      if (failedPaths.Count > 0)
      {
        LogHelper.LogWarning($"Failed remove dll files {string.Join(", ", failedPaths)}");
        return false;
      }

      return true;
    }

    public DllFilesService(FileService fileService, LocalPackagesConfigService localPackagesConfigService)
    {
      _fileService = fileService;
      _localPackagesConfigService = localPackagesConfigService;
    }
  }
}
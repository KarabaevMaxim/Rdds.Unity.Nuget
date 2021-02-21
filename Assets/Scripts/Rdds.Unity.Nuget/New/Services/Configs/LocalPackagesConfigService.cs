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
  internal class LocalPackagesConfigService
  {
    private const string ConfigName = "LocalPackages.json";
    
    private readonly FileService _fileService;
    
    private List<LocalPackageInfo> _localPackages = new List<LocalPackageInfo>();

    public IReadOnlyList<LocalPackageInfo> LocalPackages => _localPackages;
    
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
        _localPackages = JsonConvert.DeserializeObject<List<LocalPackageInfo>>(json);
      }
      catch (JsonException ex)
      {
        LogHelper.LogWarningException($"An error occurred while reading {ConfigName} file, a new file will be created", ex);
        await SaveDefaultConfigFileAsync();
      }
    }

    public async Task SaveConfigFileAsync()
    {
      var json = JsonConvert.SerializeObject(_localPackages, Formatting.Indented);
      await _fileService.WriteToFileAsync(ConfigName, json);
    }

    public string? GetPackagePath(PackageIdentity identity) =>
      _localPackages
        .FirstOrDefault(p => p.Id == identity.Id && p.Version == identity.Version.ToString())?.Path;

    public string RequirePackagePath(PackageIdentity identity)
    {
      var package = GetPackagePath(identity);

      if (package == null)
        throw new PackageNotFoundException(identity.Id, identity.Version.ToString());

      return package;
    }
    
    public void AddLocalPackage(PackageIdentity identity, string localPath) => 
      _localPackages.Add(new LocalPackageInfo(identity.Id, identity.Version.ToString(), localPath));

    private async Task SaveDefaultConfigFileAsync()
    {
      // _localPackages = new List<LocalPackageInfo>();
      _localPackages = new List<LocalPackageInfo>
      {
        new LocalPackageInfo("Newtonsoft.Json", "8.0.2", @"D:\NugetRepository\Newtonsoft.Json.8.0.2")
      };
      await SaveConfigFileAsync();
    }
    
    public LocalPackagesConfigService(FileService fileService) => _fileService = fileService;
  }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Common;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.PackagesFile;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.Services
{
  internal class PackagesFileService
  {
    private readonly FileService _fileService;
    private readonly ILogger _logger;

    private readonly string _packagesFileName = Path.Combine("NugetPackages.json");

    private Packages _packages = null!;

    public async Task SavePackagesFileAsync()
    {
      var json = JsonConvert.SerializeObject(_packages, Formatting.Indented);
      await _fileService.WriteToFileAsync(_packagesFileName, json);
    }
    
    public async Task LoadPackagesFileAsync()
    {
      var json = await _fileService.ReadFromFileAsync(_packagesFileName, CancellationToken.None);

      if (string.IsNullOrWhiteSpace(json))
      {
        _logger.LogWarning("Packages file not found. Will be create a new one.");
        await CreateDefaultPackagesFileAsync();
        return;
      }
      
      try
      {
        _packages = JsonConvert.DeserializeObject<Packages>(json!);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(
          $"Error occurred while reading packages file. Will be create a new one. Exception: {ex.GetType().Name}: {ex.Message} {ex.StackTrace}");
        await CreateDefaultPackagesFileAsync();
      }
    }

    public async Task CreateDefaultPackagesFileAsync()
    {
      _packages = new Packages(new List<Package>
      {
        new Package("Rdds.HttpClient", "1.0.0", "Gitlab", string.Empty),
        new Package("Rdds.Dto", "1.0.0", "Gitlab", string.Empty)
      });
      await SavePackagesFileAsync();
    }

    public void AddPackage(PackageIdentity identity, string sourceKey, string packageDirectoryPath)
    {
      if (!Directory.Exists(packageDirectoryPath))
        throw new ArgumentOutOfRangeException(nameof(packageDirectoryPath), "Directory not found");
      
      var foundPackage = _packages.PackagesList.FirstOrDefault(p => p.Id == identity.Id);

      if (foundPackage != null) 
         RemovePackage(foundPackage);
      
      _packages.PackagesList.Add(new Package(identity.Id, identity.Version.ToString(), sourceKey, packageDirectoryPath));
    }

    public bool RemovePackage(PackageInfo package)
    {
      var foundPackage = _packages.PackagesList.FirstOrDefault(p => p.Id == package.Identity.Id);
      
      if (foundPackage != null) 
        return RemovePackage(foundPackage);

      return false;
    }

    private bool RemovePackage(Package package) => _packages.PackagesList.Remove(package);

    public IEnumerable<(PackageIdentity, string)> RequirePackages() => 
      _packages.PackagesList.Select(p => (new PackageIdentity(p.Id, PackageVersion.Parse(p.Version)), p.Source));

    public PackagesFileService(FileService fileService, ILogger logger)
    {
      _fileService = fileService;
      _logger = logger;
    }
  }
}
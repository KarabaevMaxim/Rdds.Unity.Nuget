using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;

namespace Rdds.Unity.Nuget.Services
{
  public interface INugetService
  {
    NugetPackageSource SelectedSource { get; }
    
    void ChangeActiveSource(string key);

    Task<IEnumerable<PackageInfo>> GetPackagesAsync(string filterString, int skip, int take,
      CancellationToken cancellationToken);

    Task<PackageInfo?> GetPackageAsync(string packageId, PackageVersion version, CancellationToken cancellationToken);
    
    Task<bool> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken);

    Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken);
  }
}
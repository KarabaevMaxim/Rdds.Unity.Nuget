using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.NugetConfig;

namespace Rdds.Unity.Nuget.Services
{
  // todo Create this interface common for offline and online package sources. And create 2 implementations for offline and for online. 
  internal interface INugetService
  {
    NugetPackageSource SelectedSource { get; }
    
    void ChangeActiveSource(string key);

    Task<IEnumerable<PackageInfo>> SearchPackagesAsync(string filterString, int skip, int take,
      CancellationToken cancellationToken);

    Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, CancellationToken cancellationToken);

    Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, NugetPackageSource source,
      CancellationToken cancellationToken);
    
    Task<string?> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken);

    Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken);

    [Obsolete("It doesn't always work")]
    Task<PackageInfo> FindDependenciesAsync(PackageInfo packageInfo,
      CancellationToken cancellationToken);
  }
}
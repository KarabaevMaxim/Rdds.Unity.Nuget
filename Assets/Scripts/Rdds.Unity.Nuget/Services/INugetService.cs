using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Protocol.Core.Types;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Entities.Config;

namespace Rdds.Unity.Nuget.Services
{
  public interface INugetService
  {
    NugetPackageSource SelectedSource { get; }
    
    void ChangeActiveSource(string key);

    Task<IEnumerable<PackageInfo>> SearchPackagesAsync(string filterString, int skip, int take,
      CancellationToken cancellationToken);

    Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, CancellationToken cancellationToken);

    Task<PackageInfo?> GetPackageAsync(PackageIdentity identity, NugetPackageSource source,
      CancellationToken cancellationToken);
    
    Task<bool> DownloadPackageAsync(PackageIdentity identity, CancellationToken cancellationToken);

    Task<IEnumerable<PackageVersion>> GetPackageVersionsAsync(string packageId, CancellationToken cancellationToken);

    [Obsolete("It doesn't always work")]
    Task<PackageInfo> FindDependenciesAsync(PackageInfo packageInfo,
      CancellationToken cancellationToken);
  }
}
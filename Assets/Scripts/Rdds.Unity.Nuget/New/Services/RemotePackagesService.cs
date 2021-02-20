using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.New.Services
{
  internal class RemotePackagesService
  {
    public IEnumerable<PackageInfo> FindPackages(string filterString, int skip, int take)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<PackageVersion> FindPackageVersions(string packageId)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<FrameworkGroup> FindDependencies(PackageIdentity packageIdentity)
    {
      throw new NotImplementedException();
    }
    
    public string DownloadPackage(PackageIdentity identity)
    { 
      // загружает архив из сети
      // распаковывает его в репозиторий
      // LocalPackagesConfigService.AddLocalPackage(identity, localPath)
      throw new NotImplementedException();
    }
  }
}
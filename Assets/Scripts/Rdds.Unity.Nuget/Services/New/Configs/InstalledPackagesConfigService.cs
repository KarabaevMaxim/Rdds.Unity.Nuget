using System;
using System.Collections.Generic;
using System.Linq;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Exceptions;

namespace Rdds.Unity.Nuget.Services.New.Configs
{
  internal class InstalledPackagesConfigService : IConfigService
  {
    private readonly List<InstalledPackageInfo> _installedPackages = new List<InstalledPackageInfo>();

    public IReadOnlyList<InstalledPackageInfo> InstalledPackages => _installedPackages;
    
    public void LoadConfigFile()
    {
      throw new NotImplementedException();
    }

    public void SaveConfigFile()
    {
      throw new NotImplementedException();
    }
    
    public void AddInstalledPackage(PackageIdentity identity, IEnumerable<string> installedInAssemblies, IEnumerable<string> dllNames)
    {
      // Добавляет запись вида:
      //{
      // id: identity.Id,
      // version: identity.Version,
      // assemblies: [<assemblies>],
      // dllNames: [<dllNames>]
      //}
    }

    public void AddPackageToAssemblies(string packageId, IEnumerable<string> installedInAssemblies)
    {
    }
    
    public bool RemoveInstalledPackage(PackageIdentity identity, IEnumerable<string> assemblies)
    {
      // Находит запись с указанным id
      // Удаляет переданные assemblies из сохраненных assemblies
      // Если в списке на удаление есть сборки, которых нет в сохраненных, то бросить исключение PackageNotInstalled
      // Если в результате список сборок станет пустым, то удалить всю запись пакета
      // Если удалили всю записть, то вернуть true, иначе false
      throw new NotImplementedException();
    }

    public IEnumerable<string> RequireDllNames(string packageId)
    {
      // Возвращает свойство dllNames
      throw new NotImplementedException();
    }

    public InstalledPackageInfo RequireInstalledPackage(string packageId)
    {
      var package = GetInstalledPackage(packageId);

      if (package == null)
        throw new PackageNotInstalledException(packageId);

      return package;
    }

    public InstalledPackageInfo? GetInstalledPackage(string packageId)
    {
      return _installedPackages.FirstOrDefault(p => p.Id == packageId);
    }

    public class InstalledPackageInfo
    {
      public string Id { get; }
      
      public string Version { get; }
      
      public IEnumerable<string> InstalledInAssemblies { get; }
      
      public IEnumerable<string> DllNames { get; }

      public InstalledPackageInfo(string id, string version, IEnumerable<string> installedInAssemblies, IEnumerable<string> dllNames)
      {
        Id = id;
        Version = version;
        InstalledInAssemblies = installedInAssemblies;
        DllNames = dllNames;
      }
    }
  }
}
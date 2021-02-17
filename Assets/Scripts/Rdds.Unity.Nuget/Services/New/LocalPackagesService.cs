using System;
using System.Collections.Generic;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.Services.New
{
  internal class LocalPackagesService
  {
    public bool InstallPackage(PackageIdentity identity, List<string> assemblies)
    {
      // Ищет путь до локального хранилища пакета: var path = LocalPackagesConfigService.GetPackagePath(identity)
      // Если путь не найден, то вывести в лог сообщение и вернуть false
      // Собирает инфу из nuspec файла: var packageInfo = NuspecFileService.RequirePackageInfoFromNuspec(path);
      // Копирует dll файлы в Plugins
      // Делает запись в конфиге InstalledPackages: InstalledPackagesConfigService.AddInstalledPackage(identity, assemblies, <имена скопированных dll файлов>)
      // Правит asmdef файлы
      // Устанавливает галочки в загруженных dll
      // Рекурсивно устанавливает пакеты зависимостей, если еще не установлены
      // Вернуть true, если все хорошо
      throw new NotImplementedException();
    }

    public bool RemovePackage(PackageIdentity identity, List<string> assemblies)
    {
      // Найти список установленных: dll InstalledPackagesConfigService.RequireDllNames
      // Удалить каждую dll из зависимостей asmdef файлов указанных сборок
      // Изменить записть конфига InstalledPackages: var needRemoveDlls InstalledPackagesConfigService.RemoveInstalledPackage(identity, assemblies)
      // Если needRemoveDlls = true, то удалить все dll из Plugins
      // Вернуть true, если все хорошо
      throw new NotImplementedException();
    }

    public IEnumerable<PackageIdentity> RequireInstalledPackages()
    {
      // InstalledPackagesConfigService.InstalledPackages.Select
      // вернуть урезанную инфу по пакетам: id, version
      throw new NotImplementedException();
    }

    public PackageInfo RequireInstalledPackageInfo(string packageId)
    {
      // var package = InstalledPackagesConfigService.RequireInstalledPackage(packageId);
      // var path = LocalPackagesConfigService.RequirePackagePath(new PackageIdentity(package.Id, package.Id))
      // return NuspecFileService.RequirePackageInfoFromNuspec(path)
      throw new NotImplementedException();
    }
  }
}
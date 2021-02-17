using System;
using Rdds.Unity.Nuget.Entities;

namespace Rdds.Unity.Nuget.Services.New.Configs
{
  internal class LocalPackagesConfigService : IConfigService
  {
    public void LoadConfigFile()
    {
      // Загружает конфиг
      // Десериализует полученные данные
      // Если конфиг не найден или сломан, то создать пустой по-умолчанию
    }
    
    public string? GetPackagePath(PackageIdentity identity)
    {
      // Ищет в конфиге пакет с указанными id и версией
      // Возвращает путь до локального хранилища
      throw new NotImplementedException();
    }
    
    public string RequirePackagePath(PackageIdentity identity)
    {
      // Ищет в конфиге пакет с указанными id и версией
      // Возвращает путь до локального хранилища
      // Если пакет не найден, то выкинуть исключение PackageNotFoundException
      throw new NotImplementedException();
    }
    
    public void AddLocalPackage(PackageIdentity identity, string localPath)
    {
      // добавляет в конфиг LocalPackages.json запись вида:
      // {
      //  id: <id>,
      //  version: <version>,
      //  path: <localPath>
      // }
      throw new NotImplementedException();
    }
  }
}
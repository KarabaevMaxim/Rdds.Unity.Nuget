using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Rdds.Unity.Nuget.Entities.NugetConfig;

namespace Rdds.Unity.Nuget.New.Services.Configs
{
  public class NugetConfigService
  {
    private readonly FileService _fileService;
    private readonly string _defaultGlobalConfigFilePath
      = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config");
    
    private NugetConfigFile _configFile = null!;

    // todo temp
    public string LocalRepositoryPath => @"D:\NugetRepository"; //_configFile.RepositoryPath;
    
    public IEnumerable<string> RequireAvailableSourcesKeys() => _configFile.PackageSources.Select(ps => ps.Key);
    
    public IEnumerable<NugetPackageSource> RequireAvailableSources() => _configFile.PackageSources;

    public NugetPackageSource RequirePackageSource(string key) => _configFile.PackageSources.First(ps => ps.Key == key);
    
    public NugetPackageSource RequireDefaultPackageSource() => _configFile.PackageSources.First();

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task LoadConfigFileAsync()
    {
      var configContent = (await _fileService.ReadFromFileAsync(_defaultGlobalConfigFilePath, CancellationToken.None))!;
      // todo handle the lack of nugetconfig file
      var document = XDocument.Parse(configContent);
      var root = document.Root!;
      var sourcesNode = root.Element("packageSources")!;
      var credentialsListNode = root.Element("packageSourceCredentials")!;
      var sources = sourcesNode.Elements("add").Select(e =>
      {
        var key = e.Attribute("key")!.Value;
        var path = e.Attribute("value")!.Value;
        XElement? credentialsNode;
        
        try
        {
          credentialsNode = credentialsListNode.Element(key);
        }
        catch (XmlException)
        {
          return new NugetPackageSource
          {
            Key = key,
            Path = path,
            Credentials = null
          };
        }
        
        if (credentialsNode == null)
        {
          return new NugetPackageSource
          {
            Key = key,
            Path = path,
            Credentials = null
          }; 
        }
        
        var userName = GetValue(credentialsNode, "Username") ?? throw new NullReferenceException("Username not found");
        var clearPassword = GetValue(credentialsNode, "ClearTextPassword");
        return new NugetPackageSource
        {
          Key = key,
          Path = path,
          Credentials = new Credentials
          {
            IsPasswordClearText = true,
            Password = clearPassword,
            Username = userName
          }
        };
      });
      var repositoryPath = GetValue(root.Element("config")!, "repositoryPath")!;

      _configFile = new NugetConfigFile(sources, repositoryPath);
    }

    private string? GetValue(XElement parent, string key)
    {
      return parent
        .Elements("add")
        .First(e => e.Attribute("key")!.Value == key)
        .Attribute("value")!.Value;
    }

    public NugetConfigService(FileService fileService) => _fileService = fileService;
  }
}
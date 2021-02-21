using NuGet.Common;
using Rdds.Unity.Nuget.New.Services;
using Rdds.Unity.Nuget.New.Services.Configs;
using UnityEditor;

namespace Rdds.Unity.Nuget.Services
{
  [InitializeOnLoad]
  internal static class EditorContext
  {
    public static ILogger Logger { get; }
    public static INugetService NugetService { get; }
    public static FileService FileService { get; }
    public static NugetConfigService NugetConfigService { get; }
    public static FrameworkService FrameworkService { get; }
    public static PackagesFileService PackagesFileService { get; }
    public static InstalledPackagesService InstalledPackagesService { get; }
    public static NuspecFileService NuspecFileService { get; }
    public static AssembliesService AssembliesService { get; }
    
    
    public static DllFilesService DllFilesService { get; }
    public static LocalPackagesService LocalPackagesService { get; }
    public static RemotePackagesService RemotePackagesService { get; }
    public static InstalledPackagesConfigService InstalledPackagesConfigService { get; }
    public static LocalPackagesConfigService LocalPackagesConfigService { get; }

    static EditorContext()
    {
      Logger = new UnityConsoleLogger();
      FileService = new FileService();
      FrameworkService = new FrameworkService();
      LocalPackagesConfigService = new LocalPackagesConfigService(FileService);
      DllFilesService = new DllFilesService(FileService, LocalPackagesConfigService);
      InstalledPackagesConfigService = new InstalledPackagesConfigService(FileService);
      RemotePackagesService = new RemotePackagesService();
      AssembliesService = new AssembliesService(FileService);
      NugetConfigService = new NugetConfigService(FileService);
      NuspecFileService = new NuspecFileService(FileService);
      PackagesFileService = new PackagesFileService(FileService);
      NugetService = new NugetService(Logger, NugetConfigService, FileService, FrameworkService);
      InstalledPackagesService = new InstalledPackagesService(PackagesFileService, NugetService, NugetConfigService, NuspecFileService);
      LocalPackagesService = new LocalPackagesService(InstalledPackagesConfigService, LocalPackagesConfigService, NuspecFileService, DllFilesService, AssembliesService);

    }
  }
}
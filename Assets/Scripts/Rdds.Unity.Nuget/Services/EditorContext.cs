using System.Threading;
using NuGet.Common;
using Rdds.Unity.Nuget.Services.Configs;
using UnityEditor;

namespace Rdds.Unity.Nuget.Services
{
  [InitializeOnLoad]
  internal static class EditorContext
  {
    public static ILogger Logger { get; }
    public static FileService FileService { get; }
    public static NugetConfigService NugetConfigService { get; }
    public static FrameworkService FrameworkService { get; }
    public static NuspecFileService NuspecFileService { get; }
    public static AssembliesService AssembliesService { get; }
    
    public static DllFilesService DllFilesService { get; }
    public static LocalPackagesService LocalPackagesService { get; }
    public static RemotePackagesService RemotePackagesService { get; }
    public static InstalledPackagesConfigService InstalledPackagesConfigService { get; }
    public static LocalPackagesConfigService LocalPackagesConfigService { get; }
    
    public static SynchronizationContext MainThreadSynchContext { get; }
    
    static EditorContext()
    {
      MainThreadSynchContext = SynchronizationContext.Current;
      Logger = new UnityConsoleLogger();
      FileService = new FileService();
      FrameworkService = new FrameworkService();
      LocalPackagesConfigService = new LocalPackagesConfigService(FileService);
      DllFilesService = new DllFilesService(FileService, LocalPackagesConfigService);
      InstalledPackagesConfigService = new InstalledPackagesConfigService(FileService);
      AssembliesService = new AssembliesService(FileService, InstalledPackagesConfigService);
      NugetConfigService = new NugetConfigService(FileService);
      NuspecFileService = new NuspecFileService(FileService);
      RemotePackagesService = new RemotePackagesService(NugetConfigService, FileService, FrameworkService, LocalPackagesConfigService, Logger);
      LocalPackagesService = new LocalPackagesService(InstalledPackagesConfigService, LocalPackagesConfigService, NuspecFileService, DllFilesService, AssembliesService);
    }
  }
}
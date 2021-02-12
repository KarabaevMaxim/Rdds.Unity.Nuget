using NuGet.Common;
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
    
    static EditorContext()
    {
      Logger = new UnityConsoleLogger();
      FileService = new FileService();
      FrameworkService = new FrameworkService();
      NugetConfigService = new NugetConfigService(FileService);
      NuspecFileService = new NuspecFileService();
      PackagesFileService = new PackagesFileService(FileService);
      NugetService = new NugetService(Logger, NugetConfigService, FileService, FrameworkService);
      InstalledPackagesService = new InstalledPackagesService(PackagesFileService, NugetService, NugetConfigService, NuspecFileService);
    }
  }
}
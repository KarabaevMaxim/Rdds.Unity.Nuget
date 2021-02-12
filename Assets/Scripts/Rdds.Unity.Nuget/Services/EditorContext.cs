using NuGet.Common;

namespace Rdds.Unity.Nuget.Services
{
  internal class EditorContext
  {
    private static EditorContext? _instance;
    
    public static EditorContext Instance => _instance ??= new EditorContext();

    public ILogger Logger { get; }
    public INugetService NugetService { get; }
    public FileService FileService { get; }
    public NugetConfigService NugetConfigService { get; }
    public FrameworkService FrameworkService { get; }
    public PackagesFileService PackagesFileService { get; }
    public InstalledPackagesService InstalledPackagesService { get; }
    public NuspecFileService NuspecFileService { get; }
    
    private EditorContext()
    {
      Logger = new UnityConsoleLogger();
      FileService = new FileService();
      FrameworkService = new FrameworkService();
      NugetConfigService = new NugetConfigService(FileService);
      NuspecFileService = new NuspecFileService(Logger);
      PackagesFileService = new PackagesFileService(FileService, Logger);
      NugetService = new NugetService(Logger, NugetConfigService, FileService, FrameworkService, PackagesFileService, NuspecFileService);
      InstalledPackagesService =
        new InstalledPackagesService(PackagesFileService, NugetService, NugetConfigService, NuspecFileService, Logger);
    }
  }
}
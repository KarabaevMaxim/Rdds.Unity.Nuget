using NuGet.Common;

namespace Rdds.Unity.Nuget.Services
{
  public class EditorContext
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
    
    private EditorContext()
    {
      Logger = new UnityConsoleLogger();
      FileService = new FileService();
      FrameworkService = new FrameworkService();
      NugetConfigService = new NugetConfigService(FileService);
      PackagesFileService = new PackagesFileService(FileService, Logger);
      NugetService = new NugetService(Logger, NugetConfigService, FileService, FrameworkService);
      InstalledPackagesService =
        new InstalledPackagesService(PackagesFileService, NugetService, NugetConfigService, Logger);
    }
  }
}
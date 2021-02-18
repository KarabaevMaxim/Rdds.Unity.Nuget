using System.IO;

namespace Rdds.Unity.Nuget.New.UI
{
  public static class Paths
  {
    public static string DefaultIconResourceName { get; } = "NugetIcon";
    public static string UpdatePackageButtonIconResourceName { get; } = "PackageUpdate";
    public static string InstallPackageButtonIconResourceName { get; } = "InstallIcon";
    public static string RemovePackageButtonIconResourceName { get; } = "RemoveIcon";

    public static string LayoutRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/New/UI/Layout";
    public static string StylesRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/New/UI/Layout/Styles";
    public static string LayoutControlsRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/New/UI/Controls/Layout";
    public static string StylesControlsRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/New/UI/Controls/Layout/Styles";
    
    public static string Styles { get; } = Path.Combine(StylesRootPath, "Styles.uss");
    public static string PackageDetailsStyles { get; } = Path.Combine(StylesControlsRootPath, "PackageDetailsStyles.uss");

    public static string MainWindowLayout { get; } = Path.Combine(LayoutRootPath, "MainWindow.uxml");
    public static string PackageRowLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackageRow.uxml");
    public static string PackagesListControlLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackagesListControl.uxml");
    public static string PackageDetailControlLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackageDetailsControl.uxml");
    public static string AssemblyRowLayout { get; } = Path.Combine(LayoutControlsRootPath, "AssemblyRow.uxml");

  }
}
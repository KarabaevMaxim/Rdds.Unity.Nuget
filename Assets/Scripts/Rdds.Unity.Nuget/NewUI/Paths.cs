using System.IO;

namespace Rdds.Unity.Nuget.NewUI
{
  public static class Paths
  {
    public static string DefaultIconResourceName { get; } = "NugetIcon";
    
    public static string LayoutRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/NewUI/Layout";
    
    public static string LayoutStylesRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/NewUI/Layout/Styles";
    
    public static string LayoutControlsRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/NewUI/Layout/Controls";
    
    public static string Styles { get; } = Path.Combine(LayoutStylesRootPath, "Styles.uss");
    
    public static string PackageRowStyles { get; } = Path.Combine(LayoutStylesRootPath, "PackageRowStyles.uss");

    public static string MainWindowLayout { get; } = Path.Combine(LayoutRootPath, "MainWindow.uxml");
    
    public static string PackageRowLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackageRow.uxml");
    
    public static string PackagesListControlLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackagesListControl.uxml");
  }
}
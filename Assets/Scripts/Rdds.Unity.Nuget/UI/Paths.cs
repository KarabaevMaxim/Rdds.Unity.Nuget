using System.IO;

namespace Rdds.Unity.Nuget.UI
{
  public static class Paths
  {
    public static string LayoutRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/UI/Layout";
    
    public static string LayoutControlsRootPath { get; } = "Assets/Scripts/Rdds.Unity.Nuget/UI/Layout/Controls";

    public static string StylesRootPath { get; } = Path.Combine("Assets/Scripts/Rdds.Unity.Nuget/UI/Layout", "Styles");

    public static string MainWindowLayout { get; } = Path.Combine(LayoutRootPath, "MainWindow.uxml");
    
    public static string PackageControlLayout { get; } = Path.Combine(LayoutControlsRootPath, "PackageControl.uxml");
    
    public static string Styles { get; } = Path.Combine(StylesRootPath, "Styles.uss");
    
    public static string CommonStyles { get; } = Path.Combine(StylesRootPath, "CommonStyles.uss");
    
    public static string TabStyles { get; } = Path.Combine(StylesRootPath, "TabStyles.uss");
    
    public static string SearchTabLayout { get; } = Path.Combine(LayoutControlsRootPath, "SearchTab.uxml");
    
    public static string InstalledTabLayout { get; } = Path.Combine(LayoutControlsRootPath, "InstalledTab.uxml");
  }
}
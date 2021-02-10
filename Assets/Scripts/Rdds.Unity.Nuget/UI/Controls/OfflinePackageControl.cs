using UnityEngine.UIElements;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class OfflinePackageControl : PackageControlBase
  {
    public OfflinePackageControl(VisualElement parent, PackageInfo packageInfo) : base(parent, packageInfo)
    {
    }
  }
}
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Services;
using Rdds.Unity.Nuget.Utility;
using UnityEngine.UIElements;
using PackageInfo = Rdds.Unity.Nuget.Entities.PackageInfo;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class OfflinePackageControl : PackageControlBase
  {
    private readonly InstalledPackagesService _installedPackagesService;

    public async Task InitializeAsync()
    {
      var versionLabel = new Label(PackageInfo.Identity.Version.ToString());
      versionLabel.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
      VersionPlaceholder.Add(versionLabel);
      ActionButtonText = "Remove";
      ActionButton.clickable.clicked += OnRemoveButtonClicked;
      SetDependencies();
      await SetIconAsync(CancellationToken.None);
    }

    private async void OnRemoveButtonClicked()
    {
      var result = await _installedPackagesService.RemovePackageAsync(PackageInfo.Identity, null);

      if (result)
        RemoveFromLayout();
      else
        DialogHelper.ShowErrorAlert($"Failed to remove package {PackageInfo.Identity.Id}");
    }
    
    public OfflinePackageControl(VisualElement parent, PackageInfo packageInfo, InstalledPackagesService installedPackagesService) : base(parent, packageInfo)
    {
      _installedPackagesService = installedPackagesService;
    }
  }
}
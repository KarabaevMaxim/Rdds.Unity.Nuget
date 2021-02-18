using Rdds.Unity.Nuget.New.UI.Controls;

namespace Rdds.Unity.Nuget.New.Presenter
{
  internal class PackageDetailsPresenter
  {
    private readonly PackageDetailsControl _packageDetailsControl;

    public void Reset() => _packageDetailsControl.Reset();

    public PackageDetailsPresenter(PackageDetailsControl packageDetailsControl)
    {
      _packageDetailsControl = packageDetailsControl;
    }
  }
}
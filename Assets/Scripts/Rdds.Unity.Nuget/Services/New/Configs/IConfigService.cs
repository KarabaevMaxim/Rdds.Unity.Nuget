using System.Threading.Tasks;

namespace Rdds.Unity.Nuget.Services.New.Configs
{
  internal interface IConfigService
  {
    Task LoadConfigFileAsync();
  }
}
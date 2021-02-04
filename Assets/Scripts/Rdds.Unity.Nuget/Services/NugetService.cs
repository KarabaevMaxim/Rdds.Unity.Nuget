using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Rdds.Unity.Nuget.Services
{
  public class NugetService
  {
    public async Task<IEnumerable<PackageInfo>> GetPackages(string filterString, int skip, int take)
    {
      var logger = NullLogger.Instance;
      var cancellationToken = CancellationToken.None;

      var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
      var resource = await repository.GetResourceAsync<PackageSearchResource>();
      var searchFilter = new SearchFilter(true);

      var results = 
        await resource.SearchAsync(filterString, searchFilter, skip, take, logger, cancellationToken);

      return results.Select(r => new PackageInfo
      {
        Authors = r.Authors,
        Description = r.Description,
        DownloadCount = r.DownloadCount,
        IconUrl = r.IconUrl,
        Owners = r.Owners,
        Summary = r.Summary,
        Title = r.Title,
        Version = r.Identity.Version.OriginalVersion,
        Id = r.Identity.Id
      });
    }
  }
}
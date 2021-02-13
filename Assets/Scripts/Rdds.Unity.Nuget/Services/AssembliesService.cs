using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Utility;

namespace Rdds.Unity.Nuget.Services
{
  internal class AssembliesService
  {
    private const string CodeRootDirectory = "Assets/Scripts";
    
    private readonly FileService _fileService;

    public async Task<IEnumerable<AssemblyModel>> RequireAllAssembliesAsync()
    {
      var tasks = _fileService.FindFiles(CodeRootDirectory, "asmdef", true).Select(async f =>
      {
        var json = await _fileService.RequireFileContentAsync(f, CancellationToken.None);

        try
        {
          return JsonConvert.DeserializeObject<AssemblyModel>(json);
        }
        catch (JsonException ex)
        {
          LogHelper.LogWarningException($"Failed parse asmdef file {f}", ex);
          return null;
        }
      });

      return (await Task.WhenAll(tasks)).Where(a => a != null)!;
    }

    public AssembliesService(FileService fileService) => _fileService = fileService;
  }
}
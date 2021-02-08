using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Rdds.Unity.Nuget.Services
{
  public static class DownloadHelper
  {
    public static async Task<Texture2D?> DownloadImageAsync(Uri? url, CancellationToken token)
    {
      if (url == null)
        return null;

      using var client = new HttpClient();
      var response = await client.GetAsync(url, token);
      var bytes = await response.Content.ReadAsByteArrayAsync();
      var result = new Texture2D(0, 0);
      result.LoadImage(bytes);
      return result;
    }
  }
}
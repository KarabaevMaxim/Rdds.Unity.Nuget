using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rdds.Unity.Nuget.Services
{
  public class DownloadHelper
  {
    public static Texture2D DownloadImage(string url)
    {
      bool timedout = false;
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      WWW request = new WWW(url);
      while (!request.isDone)
      {
        if (stopwatch.ElapsedMilliseconds >= 750)
        {
          request.Dispose();
          timedout = true;
          break;
        }
      }

      Texture2D result = null;

      if (timedout)
      {
        Debug.LogWarning($"Downloading image {url} timed out! Took more than 750ms.");
      }
      else
      {
        if (string.IsNullOrEmpty(request.error))
        {
          result = request.textureNonReadable;
          Debug.LogWarning($"Downloading image {url} took {stopwatch.ElapsedMilliseconds} ms");
        }
        else
        {
          Debug.LogWarning($"Request error: {request.error}");
        }
      }

      request.Dispose();
      return result;
    }
  }
}
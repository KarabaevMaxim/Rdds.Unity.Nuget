using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using UnityEngine;

namespace Rdds.Unity.Nuget.Services
{
  internal static class ImageHelper
  {
    public static Task<Texture2D?> DownloadImageAsync(ResourcePath imagePath, CancellationToken token)
    {
      if (imagePath.IsLocalPath)
        return GetImageFromFileSystemAsync(imagePath.Path, token);

      return DownloadImageAsync(new Uri(imagePath.Path), token);
    }

    private static async Task<Texture2D?> DownloadImageAsync(Uri imageUrl, CancellationToken token)
    {
      using var client = new HttpClient();
      var response = await client.GetAsync(imageUrl, token);
      var bytes = await response.Content.ReadAsByteArrayAsync();

      if (bytes == null)
        return null;

      return RequireTextureFromBytes(bytes);
    }
    
    private static async Task<Texture2D?> GetImageFromFileSystemAsync(string imageFilePath, CancellationToken cancellationToken)
    {
      var fileService = EditorContext.Instance.FileService;
      var bytes = await fileService.ReadBytesAsync(imageFilePath, cancellationToken);

      if (bytes == null)
        return null;

      return RequireTextureFromBytes(bytes);
    }

    private static Texture2D RequireTextureFromBytes(byte[] bytes)
    {
      var result = new Texture2D(0, 0);
      result.LoadImage(bytes);
      return result;
    }
  }
}
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using UnityEngine;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class ImageHelper
  {
    public static Task<Texture?> LoadImageAsync(ResourcePath imagePath, CancellationToken token)
    {
      if (imagePath.IsLocalPath)
        return GetImageFromFileSystemAsync(imagePath.Path, token);

      return DownloadImageAsync(new Uri(imagePath.Path), token);
    }

    private static async Task<Texture?> DownloadImageAsync(Uri imageUrl, CancellationToken token)
    {
      using var client = new HttpClient();
      var response = await client.GetAsync(imageUrl, token);
      var bytes = await response.Content.ReadAsByteArrayAsync();

      if (bytes == null)
        return null;

      return RequireTextureFromBytes(bytes);
    }
    
    private static async Task<Texture?> GetImageFromFileSystemAsync(string imageFilePath, CancellationToken cancellationToken)
    {
      var fileService = EditorContext.FileService;
      var bytes = await fileService.ReadBytesAsync(imageFilePath, cancellationToken);

      if (bytes == null)
        return null;

      return RequireTextureFromBytes(bytes);
    }

    private static Texture RequireTextureFromBytes(byte[] bytes)
    {
      var result = new Texture2D(0, 0);
      result.LoadImage(bytes);
      return result;
    }
  }
}
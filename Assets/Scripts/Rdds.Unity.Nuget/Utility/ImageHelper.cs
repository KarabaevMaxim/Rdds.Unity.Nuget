using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rdds.Unity.Nuget.Entities;
using Rdds.Unity.Nuget.Services;
using UnityEditor;
using UnityEngine;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class ImageHelper
  {
    public static Task<Texture2D?> LoadImageAsync(ResourcePath imagePath, CancellationToken token)
    {
      if (imagePath.IsLocalPath)
        return GetImageFromFileSystemAsync(imagePath.Path, token);

      return DownloadImageAsync(new Uri(imagePath.Path), token);
    }

    public static Texture2D LoadImageFromResource(string resourcePath)
    {
      var result = Resources.Load<Texture2D>(resourcePath);
      
      if (result == null)
        throw new ArgumentOutOfRangeException(nameof(resourcePath), $"Texture '{resourcePath}' not found in resources");
      
      return result;
    }

    public static Texture2D LoadBuiltinImage(string name) => (EditorGUIUtility.IconContent(name).image as Texture2D)!;

    public static Texture2D RequireTextureFromBytes(byte[] bytes)
    {
      var result = new Texture2D(0, 0);
      result.LoadImage(bytes);
      return result;
    }

    public static string TextureToBase64(Texture2D texture) => Convert.ToBase64String(texture.EncodeToPNG());
    
    public static Texture2D TextureFromBase64(string base64) => RequireTextureFromBytes(Convert.FromBase64String(base64));

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
      var fileService = EditorContext.FileService;
      var bytes = await fileService.ReadBytesAsync(imageFilePath, cancellationToken);

      if (bytes == null)
        return null;

      return RequireTextureFromBytes(bytes);
    }
  }
}
using System.Threading.Tasks;
using UnityEditor;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class DialogHelper
  {
    public static void ShowErrorAlert(string message) => EditorUtility.DisplayDialog("Error", message, "Close");

    public static async Task<TResult> ShowLoadingAsync<TResult>(string title, string message, Task<TResult> task)
    {
      TResult result;
      
      try
      {
        EditorUtility.DisplayProgressBar(title, message, 0.3f);
        result = await task;
      }
      catch (TaskCanceledException)
      {
        return default!;
      }
      finally
      {
        EditorUtility.ClearProgressBar();
      }

      return result;
    }
    
    public static async Task ShowLoadingAsync(string title, string message, Task task)
    {
      try
      {
        EditorUtility.DisplayProgressBar(title, message, 0.3f);
        await task;
      }
      finally
      {
        EditorUtility.ClearProgressBar();
      }
    }
  }
}
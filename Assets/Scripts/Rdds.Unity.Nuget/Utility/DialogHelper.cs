using System.Threading.Tasks;
using UnityEditor;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class DialogHelper
  {
    public static void ShowErrorAlert(string message) => EditorUtility.DisplayDialog("Error", message, "Close");

    public static async Task<TResult> ShowLoadingAsync<TResult>(string title, string message, Task<TResult> task)
    {
      EditorUtility.DisplayProgressBar(title, message, 0);
      var result = await task;
      EditorUtility.ClearProgressBar();
      return result;
    }
  }
}
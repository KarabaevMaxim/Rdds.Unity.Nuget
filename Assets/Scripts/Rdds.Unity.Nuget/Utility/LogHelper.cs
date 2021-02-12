using System;
using NuGet.Common;
using Rdds.Unity.Nuget.Services;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class LogHelper
  {
    private static readonly ILogger _logger = EditorContext.Instance.Logger;

    public static void LogWarningException(Exception exception) => 
      _logger.LogWarning(CreateExceptionMessage(exception));

    private static string CreateExceptionMessage(Exception exception) => 
      $"{exception.GetType().Name}: {exception.Message} {exception.StackTrace}";
  }
}
using System;
using NuGet.Common;
using Rdds.Unity.Nuget.Services;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class LogHelper
  {
    private static readonly ILogger _logger = EditorContext.Logger;

    public static void LogWarning(string message) => _logger.LogWarning(message);
    
    public static void LogWarningException(Exception exception) => 
      _logger.LogWarning(CreateExceptionMessage(exception));

    public static void LogWarningException(string contextMessage, Exception exception) => 
      _logger.LogWarning($"{contextMessage}. Exception: {CreateExceptionMessage(exception)}");

    private static string CreateExceptionMessage(Exception exception) => 
      $"{exception.GetType().Name}: {exception.Message} {exception.StackTrace}";
  }
}
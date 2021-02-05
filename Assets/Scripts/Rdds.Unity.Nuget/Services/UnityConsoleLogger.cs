using System;
using System.Threading.Tasks;
using NuGet.Common;
using UnityEngine;
using ILogger = NuGet.Common.ILogger;

namespace Rdds.Unity.Nuget.Services
{
  public class UnityConsoleLogger : ILogger
  {
    public void LogDebug(string data) => LogInformation(data);

    public void LogVerbose(string data) => LogInformation(data);

    public void LogInformation(string data) => Debug.Log(data);

    public void LogMinimal(string data) => LogInformation(data);

    public void LogWarning(string data) => Debug.LogWarning(data);

    public void LogError(string data) => Debug.LogError(data);

    public void LogInformationSummary(string data) => LogInformation(data);

    public void Log(LogLevel level, string data)
    {
      switch (level)
      {
        case LogLevel.Debug:
          LogDebug(data);
          break;
        case LogLevel.Verbose:
          LogVerbose(data);
          break;
        case LogLevel.Information:
          LogInformation(data);
          break;
        case LogLevel.Minimal:
          LogMinimal(data);
          break;
        case LogLevel.Warning:
          LogWarning(data);
          break;
        case LogLevel.Error:
          LogError(data);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(level), level, null);
      }
    }

    public Task LogAsync(LogLevel level, string data) => Task.Run(() => Log(level, data));

    public void Log(ILogMessage message) => Log(message.Level, message.Message);

    public Task LogAsync(ILogMessage message) => Task.Run(() => Log(message));
  }
}
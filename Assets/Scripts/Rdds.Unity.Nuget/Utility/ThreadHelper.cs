using System;
using Rdds.Unity.Nuget.Services;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class ThreadHelper
  {
    public static void RunInMainThread(Action action) =>
      EditorContext.MainThreadSynchContext.Send(_ => action.Invoke(), null);
  }
}
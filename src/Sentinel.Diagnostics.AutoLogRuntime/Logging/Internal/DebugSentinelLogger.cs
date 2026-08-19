using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal sealed class DebugSentinelLogger : SentinelLoggerBase
{
    public static readonly DebugSentinelLogger Instance = new();

    private DebugSentinelLogger() { }

    protected override void Write(string message)
    {
        Debug.WriteLine(message);
    }
}
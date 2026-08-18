using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

internal sealed class DebugSentinelLogger : ISentinelLogger
{
    public static readonly DebugSentinelLogger Instance = new();

    private DebugSentinelLogger()
    {
    }

    public void Log(SentinelLogEvent logEvent)
    {
        Debug.WriteLine($"Log: {logEvent}");
    }

    public void LogStarted(SentinelStartEvent logEvent)
    {
        Debug.WriteLine($"LogStarted: {logEvent}");
    }

    public void LogCompleted(SentinelCompletionEvent logEvent)
    {
        Debug.WriteLine($"LogCompleted: {logEvent}");
    }

    public void LogParameter(SentinelParameterEvent logEvent)
    {
        Debug.WriteLine($"LogParameter: {logEvent}");
    }

    public void LogCallPath(SentinelCallPathEvent logEvent)
    {
        Debug.WriteLine($"LogCallPath: {logEvent}");
    }

    public void LogException(SentinelExceptionEvent logEvent)
    {
        Debug.WriteLine($"LogException: {logEvent.ToStringPretty()}");
    }
}
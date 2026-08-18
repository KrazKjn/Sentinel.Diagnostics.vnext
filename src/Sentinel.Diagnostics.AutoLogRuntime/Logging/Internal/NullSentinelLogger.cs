using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

internal sealed class NullSentinelLogger : ISentinelLogger
{
    public static readonly NullSentinelLogger Instance = new();

    private NullSentinelLogger()
    {
    }

    public void Log(SentinelLogEvent logEvent)
    {
    }

    public void LogStarted(SentinelStartEvent logEvent)
    {
    }

    public void LogCompleted(SentinelCompletionEvent logEvent)
    {
    }

    public void LogParameter(SentinelParameterEvent logEvent)
    {
    }

    public void LogCallPath(SentinelCallPathEvent logEvent)
    {
    }

    public void LogException(SentinelExceptionEvent logEvent)
    {
    }
}
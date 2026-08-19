using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public static class SentinelLogger
{
    private static readonly CompositeSentinelLogger _root = new();
    private static ISentinelLogger _current = _root;

    public static ISentinelLogger Current => _current;

    public static void Configure(ISentinelLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _root.Add(logger);
    }

    public static void Log(SentinelLogEvent logEvent)
    {
        _current.Log(logEvent);
    }

    public static void LogStarted(SentinelStartEvent logEvent)
    {
        _current.LogStarted(logEvent);
    }

    public static void LogCompleted(SentinelCompletionEvent logEvent)
    {
        _current.LogCompleted(logEvent);
    }

    public static void LogParameter(SentinelParameterEvent logEvent)
    {
        _current.LogParameter(logEvent);
    }

    public static void LogCallPath(SentinelCallPathEvent logEvent)
    {
        _current.LogCallPath(logEvent);
    }

    public static void LogException(SentinelExceptionEvent logEvent)
    {
        _current.LogException(logEvent);
    }
}
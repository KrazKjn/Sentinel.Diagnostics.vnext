using System.Collections.Generic;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal sealed class CompositeSentinelLogger : ISentinelLogger
{
    private readonly List<ISentinelLogger> _loggers = [];

    public void Add(ISentinelLogger logger)
    {
        if (logger != null)
            _loggers.Add(logger);
    }

    public void Log(SentinelLogEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.Log(logEvent);
    }

    public void LogStarted(SentinelStartEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.LogStarted(logEvent);
    }

    public void LogCompleted(SentinelCompletionEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.LogCompleted(logEvent);
    }

    public void LogParameter(SentinelParameterEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.LogParameter(logEvent);
    }

    public void LogCallPath(SentinelCallPathEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.LogCallPath(logEvent);
    }

    public void LogException(SentinelExceptionEvent logEvent)
    {
        foreach (var logger in _loggers)
            logger.LogException(logEvent);
    }
}

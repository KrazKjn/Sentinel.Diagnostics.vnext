using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events.Constants;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal abstract class SentinelLoggerBase : ISentinelLogger
{
    public static string FormatPrefix(Guid instanceId)
    {
        if (!AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
            return string.Empty;

        return string.Format(LoggingConstants.DebugPrefixFormat, DateTime.UtcNow, instanceId);
    }

    public static string ApplyIndentation(string prefix, string message, AutoLoggerLevel level, int depth)
    {
        int indentLevel = AutoLoggerConfig.IndentLevel;

        if ((AutoLoggerConfig.MinimumLevel.Level < SentinelLogLevel.Debug || AutoLoggerConfig.MinimumLevel.Verbosity < indentLevel) &&
            level.Verbosity < indentLevel)
            return $"{prefix}{message}";

        string padding = new(' ', depth * LoggingConstants.IndentationMultiplier);
        return $"{prefix}{padding}{message}";
    }

    public static string FormatMessage(string message, Guid instanceId, AutoLoggerLevel level, int depth)
    {
        string prefix = FormatPrefix(instanceId);
        return ApplyIndentation(prefix, message, level, depth);
    }

    //Default implementations call the abstract Write() method
    public virtual void Log(SentinelLogEvent logEvent)
    {
        //Write($"Log: {logEvent}");
        Write(FormatMessage(logEvent.Message, logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
    }

    public virtual void LogStarted(SentinelStartEvent logEvent)
    {
        if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
            Write(FormatMessage($"{EventConstants.FuncStartToken}{logEvent.FullName} ({logEvent.MemberType})]", logEvent.InstanceId, logEvent.Level, logEvent.Depth));
        Write(SentinelLoggerBase.FormatMessage($"LogStarted: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
    }

    public virtual void LogCompleted(SentinelCompletionEvent logEvent)
    {
        Write(SentinelLoggerBase.FormatMessage($"LogCompleted: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
            Write(SentinelLoggerBase.FormatMessage(EventConstants.FuncEndToken, logEvent.InstanceId, logEvent.Level, logEvent.Depth));
    }

    public virtual void LogParameter(SentinelParameterEvent logEvent)
    {
        Write(SentinelLoggerBase.FormatMessage($"LogParameter: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
    }

    public virtual void LogCallPath(SentinelCallPathEvent logEvent)
    {
        Write(SentinelLoggerBase.FormatMessage($"LogCallPath: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
    }

    public virtual void LogException(SentinelExceptionEvent logEvent)
    {
        Write(SentinelLoggerBase.FormatMessage($"LogException: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
    }

    // Concrete loggers override this
    protected abstract void Write(string message);
}

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

    // ============================================================================================================
    // Unified Log Event
    // ============================================================================================================

    public virtual void Log(SentinelLogEvent logEvent)
    {
        Write(
            logEvent.Level,
            FormatMessage(logEvent.Message, logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1),
            logEvent);
    }

    public virtual void LogStarted(SentinelStartEvent logEvent)
    {
        //if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
        //    Write(logEvent.Level, FormatMessage($"{EventConstants.FuncStartToken}{logEvent.FullName} ({logEvent.MemberType})]", logEvent.InstanceId, logEvent.Level, logEvent.Depth));
        //Write(logEvent.Level, SentinelLoggerBase.FormatMessage($"LogStarted: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        var evt = SentinelLogEvent.Create(
            logEvent.Level,
            $"LogStarted: {logEvent}",
            logEvent.InstanceId,
            logEvent.MethodName,
            logEvent.FullName,
            logEvent.CallPath,
            logEvent.MemberType,
            logEvent.Depth);

        Write(evt.Level, FormatMessage(evt.Message, evt.InstanceId, evt.Level, evt.Depth + 1), evt);

        if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
        {
            var debugEvt = SentinelLogEvent.Create(
                logEvent.Level,
                $"{EventConstants.FuncStartToken}{logEvent.FullName} ({logEvent.MemberType})]",
                logEvent.InstanceId,
                logEvent.MethodName,
                logEvent.FullName,
                logEvent.CallPath,
                logEvent.MemberType,
                logEvent.Depth);

            Write(debugEvt.Level, FormatMessage(debugEvt.Message, debugEvt.InstanceId, debugEvt.Level, debugEvt.Depth), debugEvt);
        }
    }

    public virtual void LogCompleted(SentinelCompletionEvent logEvent)
    {
        //Write(logEvent.Level, SentinelLoggerBase.FormatMessage($"LogCompleted: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        //if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
        //    Write(logEvent.Level, SentinelLoggerBase.FormatMessage(EventConstants.FuncEndToken, logEvent.InstanceId, logEvent.Level, logEvent.Depth));
        var evt = SentinelLogEvent.Create(
            logEvent.Level,
            $"LogCompleted: {logEvent}",
            logEvent.InstanceId,
            logEvent.MethodName,
            logEvent.FullName,
            logEvent.CallPath,
            logEvent.MemberType,
            logEvent.Depth,
            logEvent.Duration);

        Write(evt.Level, FormatMessage(evt.Message, evt.InstanceId, evt.Level, evt.Depth + 1), evt);

        if (AutoLoggerConfig.IsLevel(SentinelLogLevel.Debug))
        {
            var debugEvt = SentinelLogEvent.Create(
                logEvent.Level,
                EventConstants.FuncEndToken,
                logEvent.InstanceId,
                logEvent.MethodName,
                logEvent.FullName,
                logEvent.CallPath,
                logEvent.MemberType,
                logEvent.Depth);

            Write(debugEvt.Level, FormatMessage(debugEvt.Message, debugEvt.InstanceId, debugEvt.Level, debugEvt.Depth), debugEvt);
        }
    }

    public virtual void LogParameter(SentinelParameterEvent logEvent)
    {
        //Write(logEvent.Level, SentinelLoggerBase.FormatMessage($"LogParameter: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        var evt = SentinelLogEvent.Create(
            logEvent.Level,
            $"LogParameter: {logEvent}",
            logEvent.InstanceId,
            logEvent.MethodName,
            logEvent.FullName,
            logEvent.CallPath,
            logEvent.MemberType,
            logEvent.Depth);

        Write(evt.Level, FormatMessage(evt.Message, evt.InstanceId, evt.Level, evt.Depth + 1), evt);
    }

    public virtual void LogCallPath(SentinelCallPathEvent logEvent)
    {
        //Write(logEvent.Level, SentinelLoggerBase.FormatMessage($"LogCallPath: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        var evt = SentinelLogEvent.Create(
            logEvent.Level,
            $"LogCallPath: {logEvent}",
            logEvent.InstanceId,
            logEvent.MethodName,
            logEvent.FullName,
            logEvent.CallPath,
            logEvent.MemberType,
            logEvent.Depth);

        Write(evt.Level, FormatMessage(evt.Message, evt.InstanceId, evt.Level, evt.Depth + 1), evt);
    }

    public virtual void LogException(SentinelExceptionEvent logEvent)
    {
        //Write(logEvent.Level, SentinelLoggerBase.FormatMessage($"LogException: {logEvent}", logEvent.InstanceId, logEvent.Level, logEvent.Depth + 1));
        var evt = SentinelLogEvent.Create(
            logEvent.Level,
            $"LogException: {logEvent}",
            logEvent.InstanceId,
            logEvent.MethodName,
            logEvent.FullName,
            logEvent.CallPath,
            logEvent.MemberType,
            logEvent.Depth,
            ex: logEvent.Exception);

        Write(evt.Level, FormatMessage(evt.Message, evt.InstanceId, evt.Level, evt.Depth + 1), evt);
    }

    // ============================================================================================================
    // Concrete loggers override this
    // ============================================================================================================

    protected abstract void Write(AutoLoggerLevel autoLoggerLevel, string message, SentinelLogEvent evt);
}

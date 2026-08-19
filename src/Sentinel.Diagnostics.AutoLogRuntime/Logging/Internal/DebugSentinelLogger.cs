using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using System;
using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal sealed class DebugSentinelLogger : SentinelLoggerBase
{
    public static readonly DebugSentinelLogger Instance = new();

    private DebugSentinelLogger() { }

    protected override void Write(AutoLoggerLevel autoLoggerLevel, string message)
    {
        switch (autoLoggerLevel.Level)
        {
            case SentinelLogLevel.Trace:
                Debug.WriteLine($"TRACE: {message}");
                break;

            case SentinelLogLevel.Debug:
                Debug.WriteLine($"DEBUG: {message}");
                break;

            case SentinelLogLevel.Information:
                Debug.WriteLine($"INFO: {message}");
                break;

            case SentinelLogLevel.Warning:
                Debug.WriteLine($"WARN: {message}");
                break;

            case SentinelLogLevel.Error:
                Debug.WriteLine($"ERROR: {message}");
                break;

            case SentinelLogLevel.Critical:
                Debug.WriteLine($"CRITICAL: {message}");
                break;

            default:
                Debug.WriteLine(message);
                break;
        }
    }
}
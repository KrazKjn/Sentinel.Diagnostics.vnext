using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal sealed class ConsoleSentinelLogger : SentinelLoggerBase
{
    public static readonly ConsoleSentinelLogger Instance = new();

    private ConsoleSentinelLogger() { }

    protected override void Write(AutoLoggerLevel autoLoggerLevel, string message)
    {
        switch (autoLoggerLevel.Level)
        {
            case SentinelLogLevel.Trace:
                Console.WriteLine($"TRACE: {message}");
                break;

            case SentinelLogLevel.Debug:
                Console.WriteLine($"DEBUG: {message}");
                break;

            case SentinelLogLevel.Information:
                Console.WriteLine($"INFO: {message}");
                break;

            case SentinelLogLevel.Warning:
                Console.WriteLine($"WARN: {message}");
                break;

            case SentinelLogLevel.Error:
                Console.WriteLine($"ERROR: {message}");
                break;

            case SentinelLogLevel.Critical:
                Console.WriteLine($"CRITICAL: {message}");
                break;

            default:
                Console.WriteLine(message);
                break;
        }   
    }
}
using System;
using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

internal sealed class ConsoleSentinelLogger : ISentinelLogger
{
    public static readonly ConsoleSentinelLogger Instance = new();

    private ConsoleSentinelLogger()
    {
    }

    public void Log(SentinelLogEvent logEvent)
    {
        //Console.WriteLine($"Log: {logEvent}");
        Console.WriteLine(logEvent.Message);
    }

    public void LogStarted(SentinelStartEvent logEvent)
    {
        Console.WriteLine($"LogStarted: {logEvent}");
    }

    public void LogCompleted(SentinelCompletionEvent logEvent)
    {
        Console.WriteLine($"LogCompleted: {logEvent}");
    }

    public void LogParameter(SentinelParameterEvent logEvent)
    {
        Console.WriteLine($"LogParameter: {logEvent}");
    }

    public void LogCallPath(SentinelCallPathEvent logEvent)
    {
        Console.WriteLine($"LogCallPath: {logEvent}");
    }

    public void LogException(SentinelExceptionEvent logEvent)
    {
        Console.WriteLine($"LogException: {logEvent.ToStringPretty()}");
    }
}
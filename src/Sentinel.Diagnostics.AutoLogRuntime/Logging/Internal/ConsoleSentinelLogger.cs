using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

internal sealed class ConsoleSentinelLogger : SentinelLoggerBase
{
    public static readonly ConsoleSentinelLogger Instance = new();

    private ConsoleSentinelLogger() { }

    protected override void Write(string message)
    {
        Console.WriteLine(message);
    }
}
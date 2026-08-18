using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelLogEvent(
    SentinelLogLevel Level,
    string Message,
    int Verbosity,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    int Depth,
    TimeSpan? Duration = null)
{
    public override string ToString()
    {
        return $"{Level}: {Message} | {MethodName} | {FullName} | " +
               $"Instance={InstanceId} | Depth={Depth} | " +
               $"Duration={(Duration?.TotalMilliseconds is double ms ? $"{ms} ms" : "n/a")} | " +
               $"Path={CallPath}";
    }
}
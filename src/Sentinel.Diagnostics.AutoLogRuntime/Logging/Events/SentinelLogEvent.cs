using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public sealed record SentinelLogEvent(
    AutoLoggerLevel Level,
    string Message,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    string MemberType,
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
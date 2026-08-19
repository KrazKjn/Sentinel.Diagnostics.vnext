using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;
using System.Reflection.Emit;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public sealed record SentinelExceptionEvent(
    AutoLoggerLevel Level,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    string MemberType,
    int Depth,
    Exception Exception)
{
    public override string ToString()
    {
        return $"{MethodName} | {FullName} | " +
               $"Instance={InstanceId} | Depth={Depth} | " +
               $"Path={CallPath}" +
               $"Exception={Exception}";
    }
    public string ToStringPretty()
    {
        return $"MethodName={MethodName}\n" +
               $"FullName={FullName}\n" +
               $"Instance={InstanceId}\n" +
               $"Depth={Depth}\n" +
               $"Path={CallPath}\n" +
               $"Exception={Exception}";
    }
}
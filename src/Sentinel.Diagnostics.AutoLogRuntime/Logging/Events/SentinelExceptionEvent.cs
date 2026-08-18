using System;
using System.Reflection.Emit;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelExceptionEvent(
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
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
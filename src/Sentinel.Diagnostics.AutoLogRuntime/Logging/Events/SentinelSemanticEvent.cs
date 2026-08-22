using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public abstract record SentinelSemanticEvent
{
    public AutoLoggerLevel? Level { get; init; }
    public Guid InstanceId { get; init; }
    public string MethodName { get; init; } = "";
    public string FullName { get; init; } = "";
    public string CallPath { get; init; } = "";
    public string MemberType { get; init; } = "";
    public int Depth { get; init; }
}

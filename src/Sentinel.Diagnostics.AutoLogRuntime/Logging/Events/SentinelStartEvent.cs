using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelStartEvent(
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    int Depth);
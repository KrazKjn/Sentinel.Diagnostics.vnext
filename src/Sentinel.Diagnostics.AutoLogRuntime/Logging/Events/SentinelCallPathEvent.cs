using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelCallPathEvent(
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    int Depth);
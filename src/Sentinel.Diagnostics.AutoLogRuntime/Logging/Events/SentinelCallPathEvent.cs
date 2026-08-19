using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public sealed record SentinelCallPathEvent(
    AutoLoggerLevel Level,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    string MemberType,
    int Depth);
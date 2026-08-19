using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public sealed record SentinelParameterEvent(
    AutoLoggerLevel Level,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string ParameterName,
    Type ParameterType,
    object? Value,
    int Depth);
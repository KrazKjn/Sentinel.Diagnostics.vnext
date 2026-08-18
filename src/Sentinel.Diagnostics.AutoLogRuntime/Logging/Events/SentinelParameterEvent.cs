using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelParameterEvent(
    Guid InstanceId,
    string MethodName,
    string FullName,
    string ParameterName,
    Type ParameterType,
    object? Value,
    int Depth);
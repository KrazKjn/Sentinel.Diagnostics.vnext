using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public sealed record SentinelCompletionEvent(
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    int Depth,
    TimeSpan Duration,
    bool ExceptionLogged);
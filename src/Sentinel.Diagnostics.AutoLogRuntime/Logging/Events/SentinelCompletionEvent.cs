using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

public sealed record SentinelCompletionEvent(
    AutoLoggerLevel Level,
    Guid InstanceId,
    string MethodName,
    string FullName,
    string CallPath,
    string MemberType,
    int Depth,
    TimeSpan Duration,
    bool ExceptionLogged);
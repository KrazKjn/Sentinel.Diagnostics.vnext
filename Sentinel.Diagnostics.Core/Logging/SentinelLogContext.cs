using System;
using System.Collections.Generic;

namespace Sentinel.Diagnostics.Core.Logging;

public sealed class SentinelLogContext
{
    public DateTimeOffset StartTime { get; }
    public string MethodName { get; }
    public IReadOnlyList<object?> Parameters { get; }

    // JIRA: Add metadata reference (SentinelMethodMetadata)
    // JIRA: Add correlation ID support
    // JIRA: Add OpenTelemetry span reference
    // JIRA: Add cancellation token awareness
    public SentinelLogContext(string methodName, IReadOnlyList<object?> parameters)
    {
        MethodName = methodName;
        Parameters = parameters;
        StartTime = DateTimeOffset.UtcNow;
    }
}

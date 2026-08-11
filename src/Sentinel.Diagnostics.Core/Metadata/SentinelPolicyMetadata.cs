using System;

namespace Sentinel.Diagnostics.Core.Metadata;

public sealed class SentinelPolicyMetadata(
    string name,
    int retryCount,
    TimeSpan delay,
    TimeSpan? timeout = null)
{
    public string Name { get; } = name;
    public int RetryCount { get; } = retryCount;
    public TimeSpan Delay { get; } = delay;
    public TimeSpan? Timeout { get; } = timeout;
}

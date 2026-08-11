using System;

namespace Sentinel.Diagnostics.Policies;

public sealed class SentinelRetryPolicy : SentinelPolicy
{
    public int RetryCount { get; }
    public TimeSpan Delay { get; }

    // JIRA: Add backoff strategy reference
    // JIRA: Add jitter support
    // JIRA: Add cancellation behavior rules
    // JIRA: Add retry logging rules
    public SentinelRetryPolicy(string name, int retryCount, TimeSpan delay)
        : base(name)
    {
        RetryCount = retryCount;
        Delay = delay;
    }
}

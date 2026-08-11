using System;

namespace Sentinel.Diagnostics.Policies;

public sealed class SentinelTimeoutPolicy : SentinelPolicy
{
    public TimeSpan Timeout { get; }

    // JIRA: Add timeout cancellation token linking
    // JIRA: Add timeout logging rules
    // JIRA: Add timeout exception classification
    public SentinelTimeoutPolicy(string name, TimeSpan timeout)
        : base(name)
    {
        Timeout = timeout;
    }
}

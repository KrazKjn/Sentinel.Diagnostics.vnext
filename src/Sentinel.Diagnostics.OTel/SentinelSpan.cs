using System;

namespace Sentinel.Diagnostics.OTel;

public sealed class SentinelSpan : IDisposable
{
    public string Name { get; }

    // JIRA: Add span start timestamp
    // JIRA: Add span attributes
    // JIRA: Add span event recording
    // JIRA: Add exception recording
    // JIRA: Add retry event recording
    public SentinelSpan(string name)
    {
        Name = name;
    }

    public void Dispose()
    {
        // JIRA: Implement span completion
        // - End span
        // - Flush span data
    }
}

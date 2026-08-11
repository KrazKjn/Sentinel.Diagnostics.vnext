namespace Sentinel.Diagnostics.Policies;

public abstract class SentinelPolicy
{
    public string Name { get; }

    // JIRA: Add logging behavior flags (verbosity, dynamic logging)
    // JIRA: Add retry/timeout/circuit-breaker integration points
    protected SentinelPolicy(string name)
    {
        Name = name;
    }
}

using System;

namespace Sentinel.Diagnostics.Policies.BuiltIn;

public static class DatabasePolicies
{
    // JIRA: Add additional database policies (LongRunning, BulkOperation)
    // JIRA: Add policy tuning based on environment (Dev/QA/Prod)
    public static readonly SentinelRetryPolicy Standard =
        new("Database.Standard", retryCount: 3, delay: TimeSpan.FromMilliseconds(200));
}

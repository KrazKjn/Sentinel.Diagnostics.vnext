using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sentinel.Diagnostics.Retry;

public sealed class SentinelRetryEngine
{
    // JIRA: Implement full retry loop
    // - Apply retry policy
    // - Apply backoff strategy
    // - Apply jitter
    // - Respect cancellation token
    // - Log retry attempts
    // - Emit OpenTelemetry retry events
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int retryCount,
        TimeSpan delay,
        CancellationToken cancellationToken)
    {
        return await operation(cancellationToken);
    }
}

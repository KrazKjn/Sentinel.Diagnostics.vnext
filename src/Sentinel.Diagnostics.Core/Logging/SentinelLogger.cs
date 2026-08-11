using System;
using System.Diagnostics;

namespace Sentinel.Diagnostics.Core.Logging;

public sealed class SentinelLogger
{
    // JIRA: Implement logging pipeline (info, warning, error)
    // - Integrate with SentinelLogScope and SentinelAsyncLogScope
    // - Add policy-aware logging (verbosity, dynamic logging)
    // - Add OpenTelemetry event emission
    // - Add structured logging support (key/value pairs)
    public static void LogInfo(string message) {
        Debug.WriteLine($"INFO: {message}");
    }

    public static void LogWarning(string message) {
        Debug.WriteLine($"WARNING: {message}");
    }

    public static void LogError(string message, Exception ex) {
        Debug.WriteLine($"ERROR: {message} - Exception: {ex}");
    }
}

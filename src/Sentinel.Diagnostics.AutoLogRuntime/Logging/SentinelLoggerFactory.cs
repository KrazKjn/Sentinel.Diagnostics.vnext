using Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;
using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;
internal static class SentinelLoggerFactory
{
    public static ISentinelLogger Create(string spec)
    {
        if (string.Equals(spec, LoggingConstants.ConsoleLoggerName, StringComparison.OrdinalIgnoreCase))
            return ConsoleSentinelLogger.Instance;

        if (string.Equals(spec, LoggingConstants.DebugLoggerName, StringComparison.OrdinalIgnoreCase))
            return DebugSentinelLogger.Instance;

        if (spec.StartsWith(LoggingConstants.FileLoggerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var path = spec.Substring(LoggingConstants.FileLoggerPrefix.Length);
            return new FileSentinelLogger(path);
        }

        if (spec.StartsWith(LoggingConstants.JsonLoggerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var path = spec.Substring(LoggingConstants.JsonLoggerPrefix.Length);
            return new StructuredJsonSentinelLogger(path);
        }

        throw new InvalidOperationException($"Unknown logger engine: {spec}");
    }
}

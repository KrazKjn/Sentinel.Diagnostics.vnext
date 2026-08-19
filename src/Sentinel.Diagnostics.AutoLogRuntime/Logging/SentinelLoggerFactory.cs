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

        if (spec.StartsWith(LoggingConstants.Log4NetLoggerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Example specs:
            // "Log4Net"
            // "Log4Net:log4net.config"
            // "Log4Net:C:\\configs\\log4net.xml"

            string? configPath = null;

            var parts = spec.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
                configPath = parts[1];

            return new Log4NetSentinelLogger(configPath);
        }

        if (spec.StartsWith(LoggingConstants.SeriLogLoggerPreFix, StringComparison.OrdinalIgnoreCase))
        {
            // Example specs:
            // "SeriLog"
            // "SeriLog:SeriLog.json"
            // "SeriLog:C:\\configs\\SeriLog.json"

            string? configPath = null;

            var parts = spec.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
                configPath = parts[1];

            return new SerilogSentinelLogger(configPath);
        }

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

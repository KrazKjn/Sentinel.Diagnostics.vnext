namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants
{
    /// <summary>
    /// Centralized constants for the Sentinel logging subsystem.
    /// </summary>
    internal static class LoggingConstants
    {
        // ------------------------------------------------------------
        // Logger Engine Identifiers
        // ------------------------------------------------------------
        public const string ConsoleLoggerName = "Console";
        public const string DebugLoggerName = "Debug";
        public const string FileLoggerPrefix = "File:";
        public const string JsonLoggerPrefix = "Json:";

        // ------------------------------------------------------------
        // Debug Prefix Formatting
        // ------------------------------------------------------------
        public const string DebugPrefixFormat = "{0:o} {1}:";
        // Used with: string.Format(DebugPrefixFormat, DateTime.UtcNow, instanceId)

        // ------------------------------------------------------------
        // Indentation
        // ------------------------------------------------------------
        public const int DefaultIndentLevel = 4;
        public const int IndentationMultiplier = 3;
        // Used with: depth * IndentationMultiplier

        // ------------------------------------------------------------
        // Structured JSON Envelope Keys
        // ------------------------------------------------------------
        public const string JsonTimestampKey = "Timestamp";
        public const string JsonMessageKey = "Message";

        // ------------------------------------------------------------
        // Logger Configuration Keys (AutoLogger.json)
        // ------------------------------------------------------------
        public const string LoggersConfigKey = "Loggers";
    }
}

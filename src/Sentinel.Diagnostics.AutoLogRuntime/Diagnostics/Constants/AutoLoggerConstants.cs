namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics.Constants
{
    /// <summary>
    /// Centralized constants for the AutoLogger diagnostics subsystem.
    /// </summary>
    internal static class AutoLoggerConstants
    {
        // ------------------------------------------------------------
        // Configuration Section Names (AutoLogger.json)
        // ------------------------------------------------------------
        public const string AutoLoggerSectionName = "AutoLogger";
        public const string MinimumLevelKey = "MinimumLevel";
        public const string WarningThresholdKey = "WarningThresholdMs";
        public const string IndentLevelKey = "IndentLevel";
        public const string NamespaceLevelsKey = "NamespaceLevels";
        public const string ClassLevelsKey = "ClassLevels";
        public const string MethodLevelsKey = "MethodLevels";

        // ------------------------------------------------------------
        // Verbosity / Depth Rules
        // ------------------------------------------------------------
        public const int DefaultVerbosity = 1;
        public const int VerbosityNotSet = -1;
        public const int WarningThresholdMsNotSet = -1;
        public const int DefaultDepthIncrement = 1;

        // ------------------------------------------------------------
        // Level Resolution Constants
        // ------------------------------------------------------------
        public const string LevelPropertyName = "Level";
        public const string VerbosityPropertyName = "Verbosity";

        // ------------------------------------------------------------
        // Default Values
        // ------------------------------------------------------------
        public const long DefaultWarningThresholdMs = 250;
    }
}

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events.Constants
{
    /// <summary>
    /// Centralized constants for Sentinel structured logging events.
    /// </summary>
    internal static class EventConstants
    {
        // ------------------------------------------------------------
        // Event Formatting Tokens
        // ------------------------------------------------------------
        public const string FuncStartToken = "[Func: ";
        public const string FuncEndToken = "[End Func]";
        public const string ParamTokenPrefix = "[Param: ";
        public const string CallPathTokenPrefix = "[CallPath: ";

        // ------------------------------------------------------------
        // Event JSON Keys (Structured Event Model)
        // ------------------------------------------------------------
        public const string EventTypeKey = "EventType";
        public const string FullNameKey = "FullName";
        public const string MemberTypeKey = "MemberType";
        public const string ParameterNameKey = "ParameterName";
        public const string ParameterValueKey = "Value";
        public const string CallPathKey = "CallPath";
        public const string ExceptionKey = "Exception";
        public const string StackTraceKey = "StackTrace";

        // ------------------------------------------------------------
        // Event Type Identifiers
        // ------------------------------------------------------------
        public const string StartEventType = "Start";
        public const string CompletionEventType = "Completed";
        public const string ParameterEventType = "Parameter";
        public const string CallPathEventType = "CallPath";
        public const string ExceptionEventType = "Exception";
        public const string LogEventType = "Log";
    }
}

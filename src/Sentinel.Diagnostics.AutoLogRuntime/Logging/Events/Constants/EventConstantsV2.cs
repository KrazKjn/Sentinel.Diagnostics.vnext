namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events.Constants
{
    /// <summary>
    /// Centralized constants for Sentinel structured logging events.
    /// </summary>
    internal static class EventConstantsV2
    {
        // ------------------------------------------------------------
        // Core Event Keys
        // ------------------------------------------------------------
        public const string EventTypeKey = "EventType";
        public const string MessageKey = "Message";
        public const string InstanceIdKey = "InstanceId";
        public const string DepthKey = "Depth";
        public const string DurationKey = "Duration";

        // ------------------------------------------------------------
        // Caller Info
        // ------------------------------------------------------------
        public const string CallerFilePathKey = "CallerFilePath";
        public const string CallerLineNumberKey = "CallerLineNumber";
        public const string CallerMemberNameKey = "CallerMemberName";

        // ------------------------------------------------------------
        // Thread / Task Context
        // ------------------------------------------------------------
        public const string ThreadIdKey = "ThreadId";
        public const string ManagedThreadIdKey = "ManagedThreadId";
        public const string IsThreadPoolThreadKey = "IsThreadPoolThread";
        public const string TaskIdKey = "TaskId";
        public const string ParentTaskIdKey = "ParentTaskId";
        public const string IsAsyncKey = "IsAsync";

        // ------------------------------------------------------------
        // Machine / Process Context
        // ------------------------------------------------------------
        public const string MachineNameKey = "MachineName";
        public const string ProcessIdKey = "ProcessId";
        public const string ProcessNameKey = "ProcessName";
        public const string OSVersionKey = "OSVersion";
        public const string RuntimeVersionKey = "RuntimeVersion";

        // ------------------------------------------------------------
        // Correlation Context
        // ------------------------------------------------------------
        public const string CorrelationIdKey = "CorrelationId";
        public const string OperationIdKey = "OperationId";
        public const string ParentOperationIdKey = "ParentOperationId";

        // ------------------------------------------------------------
        // Execution Context
        // ------------------------------------------------------------
        public const string AppDomainIdKey = "AppDomainId";
        public const string AppDomainFriendlyNameKey = "AppDomainFriendlyName";
        public const string CultureKey = "Culture";
        public const string UICultureKey = "UICulture";

        // ------------------------------------------------------------
        // Exception Context
        // ------------------------------------------------------------
        public const string ExceptionTypeKey = "ExceptionType";
        public const string ExceptionMessageKey = "ExceptionMessage";
        public const string InnerExceptionTypeKey = "InnerExceptionType";
        public const string InnerExceptionMessageKey = "InnerExceptionMessage";

        // ------------------------------------------------------------
        // Performance Context
        // ------------------------------------------------------------
        public const string StartTimestampKey = "StartTimestamp";
        public const string EndTimestampKey = "EndTimestamp";
        public const string ElapsedMsKey = "ElapsedMs";
        public const string ElapsedTicksKey = "ElapsedTicks";

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

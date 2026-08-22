using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

/// <summary>
/// Represents a fully enriched diagnostic logging event emitted by the Sentinel AutoLogger runtime.
/// 
/// This event captures:
/// - Core logging metadata (level, message, instance, depth, duration)
/// - Semantic method location (method name, full type name, call path, member type)
/// - Compiler-provided caller information (file path, line number, member name)
/// - Thread and task execution context (thread identity, async state, task identity)
/// - Machine and process environment context (host, OS, runtime, process identity)
/// - Correlation and operation identifiers for distributed and nested tracing
/// - Execution environment context (AppDomain, culture, UI culture)
/// - Exception metadata (types, messages, inner exception details)
/// - Performance timing metrics (timestamps, elapsed time)
///
/// All non-semantic fields are populated automatically by <see cref="Create"/>.
/// Semantic fields are supplied by the AutoLogger instrumentation layer.
/// </summary>
public sealed record SentinelLogEvent
(
    // ================================================================================================================
    // CORE LOGGING FIELDS
    // ================================================================================================================

    /// <summary>
    /// The severity level of the log event (Trace, Debug, Information, Warning, Error, Critical).
    /// Populated directly from the AutoLogger invocation.
    /// </summary>
    AutoLoggerLevel Level,

    /// <summary>
    /// The human-readable log message produced by the AutoLogger.
    /// </summary>
    string Message,

    /// <summary>
    /// A unique identifier assigned to the instrumented method instance.
    /// Used to correlate multiple log events belonging to the same method invocation.
    /// Populated from AutoLogger metadata.
    /// </summary>
    Guid InstanceId,

    /// <summary>
    /// The depth of the call within the instrumented call graph.
    /// Depth = 0 for top-level calls; increments for nested calls.
    /// Populated from AutoLogger metadata.
    /// </summary>
    int Depth,

    /// <summary>
    /// The duration of the method execution, if available.
    /// Populated by AutoLogger exit instrumentation.
    /// </summary>
    TimeSpan? Duration = null,


    // ================================================================================================================
    // SEMANTIC LOCATION (ORIGINAL FIELDS)
    // ================================================================================================================

    /// <summary>
    /// The simple method name (e.g., "GetCustomer").
    /// Populated from AutoLogger metadata.
    /// </summary>
    string MethodName = "",

    /// <summary>
    /// The fully qualified type name containing the method (e.g., "MyApp.Services.CustomerService").
    /// Populated from AutoLogger metadata.
    /// </summary>
    string FullName = "",

    /// <summary>
    /// The hierarchical call path representing nested AutoLogger calls.
    /// Populated from AutoLogger metadata.
    /// </summary>
    string CallPath = "",

    /// <summary>
    /// The member type (Method, Constructor, PropertyGetter, PropertySetter).
    /// Populated from AutoLogger metadata.
    /// </summary>
    string MemberType = "",


    // ================================================================================================================
    // CALLER INFO (COMPILER-PROVIDED)
    // ================================================================================================================

    /// <summary>
    /// The full file path of the source code file where the log event was emitted.
    /// Automatically populated by the compiler via <see cref="CallerFilePathAttribute"/>.
    /// </summary>
    string CallerFilePath = "",

    /// <summary>
    /// The line number in the source file where the log event was emitted.
    /// Automatically populated by the compiler via <see cref="CallerLineNumberAttribute"/>.
    /// </summary>
    int CallerLineNumber = 0,

    /// <summary>
    /// The member name (method, property, etc.) where the log event was emitted.
    /// Automatically populated by the compiler via <see cref="CallerMemberNameAttribute"/>.
    /// </summary>
    string CallerMemberName = "",


    // ================================================================================================================
    // THREAD / TASK CONTEXT
    // ================================================================================================================

    /// <summary>
    /// The operating system thread identifier.
    /// Populated from <see cref="Thread.CurrentThread"/>.
    /// </summary>
    int ThreadId = 0,

    /// <summary>
    /// The managed thread identifier assigned by the .NET runtime.
    /// Populated from <see cref="Thread.ManagedThreadId"/>.
    /// </summary>
    int ManagedThreadId = 0,

    /// <summary>
    /// Indicates whether the current thread is a thread pool thread.
    /// Populated from <see cref="Thread.IsThreadPoolThread"/>.
    /// </summary>
    bool IsThreadPoolThread = false,

    /// <summary>
    /// The current task identifier if executing within an async context.
    /// Populated from <see cref="Task.CurrentId"/>.
    /// </summary>
    int TaskId = 0,

    /// <summary>
    /// The parent task identifier, if determinable.
    /// Currently always 0 because .NET does not reliably expose parent task relationships.
    /// </summary>
    int ParentTaskId = 0,

    /// <summary>
    /// Indicates whether the current execution context is asynchronous.
    /// Populated based on whether <see cref="Task.CurrentId"/> has a value.
    /// </summary>
    bool IsAsync = false,


    // ================================================================================================================
    // MACHINE / PROCESS CONTEXT
    // ================================================================================================================

    /// <summary>
    /// The machine name of the host executing the instrumented code.
    /// Populated from <see cref="Environment.MachineName"/>.
    /// </summary>
    string MachineName = "",

    /// <summary>
    /// The process identifier of the running application.
    /// Populated from <see cref="Process.GetCurrentProcess"/>.
    /// </summary>
    int ProcessId = 0,

    /// <summary>
    /// The name of the running process.
    /// Populated from <see cref="Process.ProcessName"/>.
    /// </summary>
    string ProcessName = "",

    /// <summary>
    /// The operating system version string.
    /// Populated from <see cref="Environment.OSVersion"/>.
    /// </summary>
    string OSVersion = "",

    /// <summary>
    /// The .NET runtime version.
    /// Populated from <see cref="Environment.Version"/>.
    /// </summary>
    string RuntimeVersion = "",


    // ================================================================================================================
    // CORRELATION CONTEXT
    // ================================================================================================================

    /// <summary>
    /// A correlation identifier used to group all log events belonging to a single logical request or workflow.
    /// Populated by the AutoLogger runtime using an <see cref="AsyncLocal{T}"/> value.
    /// </summary>
    Guid CorrelationId = default,

    /// <summary>
    /// A unique identifier representing the current operation (method invocation).
    /// Populated by AutoLogger entry instrumentation.
    /// </summary>
    Guid OperationId = default,

    /// <summary>
    /// The operation identifier of the caller (parent method).
    /// Enables reconstruction of nested call hierarchies.
    /// Populated by AutoLogger entry instrumentation.
    /// </summary>
    Guid ParentOperationId = default,


    // ================================================================================================================
    // EXECUTION CONTEXT
    // ================================================================================================================

    /// <summary>
    /// The AppDomain identifier.
    /// Populated from <see cref="AppDomain.CurrentDomain"/>.
    /// </summary>
    int AppDomainId = 0,

    /// <summary>
    /// The friendly name of the AppDomain.
    /// Populated from <see cref="AppDomain.CurrentDomain.FriendlyName"/>.
    /// </summary>
    string AppDomainFriendlyName = "",

    /// <summary>
    /// The current thread culture (e.g., "en-US").
    /// Populated from <see cref="Thread.CurrentThread.CurrentCulture"/>.
    /// </summary>
    string Culture = "",

    /// <summary>
    /// The current thread UI culture (e.g., "en-US").
    /// Populated from <see cref="Thread.CurrentThread.CurrentUICulture"/>.
    /// </summary>
    string UICulture = "",


    // ================================================================================================================
    // EXCEPTION CONTEXT
    // ================================================================================================================

    /// <summary>
    /// The full type name of the thrown exception, if any.
    /// Populated from <paramref name="ex"/>.
    /// </summary>
    string ExceptionType = "",

    /// <summary>
    /// The exception message, if any.
    /// Populated from <paramref name="ex"/>.
    /// </summary>
    string ExceptionMessage = "",

    /// <summary>
    /// The full type name of the inner exception, if present.
    /// Populated from <paramref name="ex.InnerException"/>.
    /// </summary>
    string InnerExceptionType = "",

    /// <summary>
    /// The inner exception message, if present.
    /// Populated from <paramref name="ex.InnerException"/>.
    /// </summary>
    string InnerExceptionMessage = "",


    // ================================================================================================================
    // PERFORMANCE CONTEXT
    // ================================================================================================================

    /// <summary>
    /// The timestamp marking the beginning of the operation.
    /// Currently unused; reserved for future high-resolution timing support.
    /// </summary>
    DateTime? StartTimestamp = null,

    /// <summary>
    /// The timestamp marking the end of the operation.
    /// Currently unused; reserved for future high-resolution timing support.
    /// </summary>
    DateTime? EndTimestamp = null,

    /// <summary>
    /// The total elapsed time in milliseconds for the operation.
    /// Populated from <paramref name="duration"/>.
    /// </summary>
    double ElapsedMs = 0,

    /// <summary>
    /// The total elapsed time in ticks for the operation.
    /// Populated from <paramref name="duration"/>.
    /// </summary>
    long ElapsedTicks = 0
)
{
    /// <summary>
    /// Populates all runtime context fields automatically.
    /// Call this immediately after constructing the event.
    /// </summary>
    public static SentinelLogEvent Create(
        AutoLoggerLevel? level,
        string message,
        Guid instanceId,
        string methodName,
        string fullName,
        string callPath,
        string memberType,
        int depth,
        TimeSpan? duration = null,
        Exception? ex = null,
        Guid? correlationId = null,
        Guid? operationId = null,
        Guid? parentOperationId = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = ""
    )
    {
        var thread = Thread.CurrentThread;
        var process = Process.GetCurrentProcess();

        return new SentinelLogEvent(
            level,
            message,
            instanceId,
            depth,
            duration,

            methodName,
            fullName,
            callPath,
            memberType,

            // Caller Info
            callerFilePath,
            callerLineNumber,
            callerMemberName,

            // Thread / Task Context
            ThreadId: thread.ManagedThreadId, // OS thread ID not always available
            ManagedThreadId: thread.ManagedThreadId,
            IsThreadPoolThread: thread.IsThreadPoolThread,
            TaskId: Task.CurrentId ?? 0,
            ParentTaskId: 0, // cannot be reliably determined
            IsAsync: Task.CurrentId.HasValue,

            // Machine / Process Context
            MachineName: Environment.MachineName,
            ProcessId: process.Id,
            ProcessName: process.ProcessName,
            OSVersion: Environment.OSVersion.VersionString,
            RuntimeVersion: Environment.Version.ToString(),

            // Correlation Context
            CorrelationId: correlationId ?? Guid.Empty,
            OperationId: operationId ?? Guid.Empty,
            ParentOperationId: parentOperationId ?? Guid.Empty,

            // Execution Context
            AppDomainId: AppDomain.CurrentDomain.Id,
            AppDomainFriendlyName: AppDomain.CurrentDomain.FriendlyName,
            Culture: Thread.CurrentThread.CurrentCulture.Name,
            UICulture: Thread.CurrentThread.CurrentUICulture.Name,

            // Exception Context
            ExceptionType: ex?.GetType().FullName ?? "",
            ExceptionMessage: ex?.Message ?? "",
            InnerExceptionType: ex?.InnerException?.GetType().FullName ?? "",
            InnerExceptionMessage: ex?.InnerException?.Message ?? "",

            // Performance Context
            StartTimestamp: null,
            EndTimestamp: null,
            ElapsedMs: duration?.TotalMilliseconds ?? 0,
            ElapsedTicks: duration?.Ticks ?? 0
        );
    }

    public override string ToString()
    {
        var durationText = Duration?.TotalMilliseconds is double ms
            ? $"{ms} ms"
            : "n/a";

        return $"{Level}: {Message} | {MethodName} | {FullName} | " +
               $"MemberType={MemberType} | Instance={InstanceId} | Depth={Depth} | " +
               $"Duration={durationText} | Path={CallPath}";
    }
}

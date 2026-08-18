using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

/// <summary>
/// Runtime diagnostic scope created by generated AutoLog instrumentation.
///
/// AutoLogger contains no reflection-based method discovery and has no
/// dependency on a third-party logging framework.
///
/// The generated instrumentation supplies all method metadata required
/// to produce the diagnostic envelope.
/// </summary>
public sealed class AutoLogger : IDisposable
{
    private readonly Guid _instanceId;
    private readonly Stopwatch _stopwatch;
    private readonly int _depth;
    private readonly IReadOnlyList<AutoLogParameter> _parameters;
    private readonly string _methodName;
    private readonly string _fullName;
    private readonly string _callPath;

    private bool _disposed;
    private bool _exceptionLogged;

    /// <summary>
    /// Creates a diagnostic scope using compile-time generated metadata.
    /// </summary>
    public AutoLogger(AutoLogMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        _instanceId = metadata.InstanceId;
        _methodName = metadata.MethodName;
        _fullName = metadata.FullName;
        _callPath = metadata.CallPath;
        _depth = metadata.Depth;
        _parameters = metadata.Parameters;

        _stopwatch = Stopwatch.StartNew();

        LogFunctionEntry();
        LogParameters();
        LogCallPath();
    }

    /// <summary>
    /// Records an exception that escaped the instrumented method.
    ///
    /// The exception is not swallowed or modified. The generated catch block
    /// is responsible for rethrowing it with "throw;".
    /// </summary>
    public void LogException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (_disposed)
        {
            return;
        }

        _exceptionLogged = true;

        SentinelLogger.LogException(
            new SentinelExceptionEvent(
                InstanceId: _instanceId,
                MethodName: _methodName,
                FullName: _fullName,
                CallPath: _callPath,
                Depth: _depth,
                Exception: exception));
        LogExceptionDetail(exception);
    }

    public void LogExceptionDetail(Exception ex)
    {
        Info("=== AUTOLOG EXCEPTION DETAIL ===");
        Info($"Exception: {ex.GetType().Name}: {ex.Message}");
        Info($"Method: {_methodName}");
        Info($"FullName: {_fullName}");
        Info($"InstanceId: {_instanceId}");
        Info($"Depth: {_depth}");
        Info($"CallPath: {_callPath}");
        Info($"Duration: {_stopwatch?.ElapsedMilliseconds ?? 0} ms");

        Info("Parameters:");
        foreach (var p in _parameters)
        {
            Info($"  {p.Name} ({p.Type.Name}) = {p.Value}");
        }

        Info("=== END AUTOLOG EXCEPTION DETAIL ===");
    }

    /// <summary>
    /// Adds a diagnostic message associated with this function invocation.
    ///
    /// This allows developers to add additional diagnostics inside an
    /// instrumented method without knowing which logging framework is
    /// configured by the application.
    /// </summary>
    public void Log(
        string message,
        SentinelLogLevel level = SentinelLogLevel.Debug)
    {
        if (_disposed)
        {
            return;
        }

        SentinelLogger.Log(
            new SentinelLogEvent(
                Level: level,
                Message: message,
                InstanceId: _instanceId,
                MethodName: _methodName,
                FullName: _fullName,
                CallPath: _callPath,
                Depth: _depth));
    }

    /// <summary>
    /// Logs a debug diagnostic message.
    /// </summary>
    public void Debug(string message)
    {
        Log(message, SentinelLogLevel.Debug);
    }

    /// <summary>
    /// Logs an informational diagnostic message.
    /// </summary>
    public void Info(string message)
    {
        Log(message, SentinelLogLevel.Information);
    }

    /// <summary>
    /// Logs a warning diagnostic message.
    /// </summary>
    public void Warning(string message)
    {
        Log(message, SentinelLogLevel.Warning);
    }

    /// <summary>
    /// Logs an error diagnostic message.
    /// </summary>
    public void Error(string message)
    {
        Log(message, SentinelLogLevel.Error);
    }

    /// <summary>
    /// Ends the diagnostic scope and records execution duration.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _stopwatch.Stop();

        var elapsed = _stopwatch.Elapsed;

        if (elapsed.TotalMilliseconds >= AutoLoggerConfig.WarningThresholdMs)
        {
            SentinelLogger.Log(
                new SentinelLogEvent(
                    Level: SentinelLogLevel.Warning,
                    Message:
                        $"Method exceeded execution threshold: {elapsed.TotalMilliseconds:F2} ms",
                    InstanceId: _instanceId,
                    MethodName: _methodName,
                    FullName: _fullName,
                    CallPath: _callPath,
                    Depth: _depth,
                    Duration: elapsed));
        }

        SentinelLogger.LogCompleted(
            new SentinelCompletionEvent(
                InstanceId: _instanceId,
                MethodName: _methodName,
                FullName: _fullName,
                CallPath: _callPath,
                Depth: _depth,
                Duration: elapsed,
                ExceptionLogged: _exceptionLogged));

        AutoLoggerContext.DecrementDepth();
    }

    private void LogFunctionEntry()
    {
        SentinelLogger.LogStarted(
            new SentinelStartEvent(
                InstanceId: _instanceId,
                MethodName: _methodName,
                FullName: _fullName,
                CallPath: _callPath,
                Depth: _depth));
    }

    private void LogParameters()
    {
        if (_parameters.Count == 0)
        {
            return;
        }

        foreach (var parameter in _parameters)
        {
            if (!parameter.ShouldLog)
            {
                continue;
            }

            if (parameter.IsSensitive)
            {
                continue;
            }

            SentinelLogger.LogParameter(
                new SentinelParameterEvent(
                    InstanceId: _instanceId,
                    MethodName: _methodName,
                    FullName: _fullName,
                    ParameterName: parameter.Name,
                    ParameterType: parameter.Type,
                    Value: parameter.Value,
                    Depth: _depth));
        }
    }

    private void LogCallPath()
    {
        if (string.IsNullOrWhiteSpace(_callPath))
        {
            return;
        }

        SentinelLogger.LogCallPath(
            new SentinelCallPathEvent(
                InstanceId: _instanceId,
                MethodName: _methodName,
                FullName: _fullName,
                CallPath: _callPath,
                Depth: _depth));
    }
}
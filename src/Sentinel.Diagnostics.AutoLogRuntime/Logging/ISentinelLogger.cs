using Sentinel.Diagnostics.AutoLogRuntime.Logging.Events;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

public interface ISentinelLogger
{
    void Log(SentinelLogEvent logEvent);

    void LogStarted(SentinelStartEvent logEvent);

    void LogCompleted(SentinelCompletionEvent logEvent);

    void LogParameter(SentinelParameterEvent logEvent);

    void LogCallPath(SentinelCallPathEvent logEvent);

    void LogException(SentinelExceptionEvent logEvent);
}
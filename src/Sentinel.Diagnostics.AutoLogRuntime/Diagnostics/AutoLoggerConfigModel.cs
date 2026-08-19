using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics.Constants;
using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants;
using System.Collections.Generic;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

public sealed class AutoLoggerRootConfig
{
    public AutoLoggerConfigModel? AutoLogger { get; set; }
}

public sealed class AutoLoggerLevel
{
    public SentinelLogLevel Level { get; set; }
    public int Verbosity { get; set; } = AutoLoggerConstants.VerbosityNotSet;
    public long WarningThresholdMs { get; set; } = AutoLoggerConstants.WarningThresholdMsNotSet;
}

public sealed class AutoLoggerConfigModel
{
    public AutoLoggerLevel? MinimumLevel { get; set; }
    public int IndentLevel { get; set; } = LoggingConstants.DefaultIndentLevel;

    public List<string>? Loggers { get; set; }

    public Dictionary<string, AutoLoggerLevel>? NamespaceLevels { get; set; }
    public Dictionary<string, AutoLoggerLevel>? ClassLevels { get; set; }
    public Dictionary<string, AutoLoggerLevel>? MethodLevels { get; set; }
}
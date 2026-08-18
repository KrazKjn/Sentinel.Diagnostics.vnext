using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using System.Collections.Generic;

public sealed class AutoLoggerRootConfig
{
    public AutoLoggerConfigModel AutoLogger { get; set; }
}

public sealed class AutoLoggerLevel
{
    public SentinelLogLevel Level { get; set; }
    public int Verbosity { get; set; } = -1;
}

public sealed class AutoLoggerConfigModel
{
    public AutoLoggerLevel MinimumLevel { get; set; }
    public long WarningThresholdMs { get; set; }

    public Dictionary<string, AutoLoggerLevel> NamespaceLevels { get; set; }
    public Dictionary<string, AutoLoggerLevel> ClassLevels { get; set; }
    public Dictionary<string, AutoLoggerLevel> MethodLevels { get; set; }
}
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics.Constants;
using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

public static class AutoLoggerConfig
{
    private static FileSystemWatcher? _watcher;
    private static string? _configPath;

    //
    // SYSTEM‑WIDE SETTINGS
    //

    /// <summary>
    /// Minimum log level for the entire application.
    /// Debug < Info < Warn < Error
    /// </summary>
    public static AutoLoggerLevel MinimumLevel { get; set; } = new AutoLoggerLevel
    {
        Level = SentinelLogLevel.Information,
        Verbosity = AutoLoggerConstants.DefaultVerbosity,
        WarningThresholdMs = AutoLoggerConstants.DefaultWarningThresholdMs
    };


    /// <summary>
    /// Verbosity level to Indent logging function levels.
    /// </summary>
    public static int IndentLevel { get; set; } = LoggingConstants.DefaultIndentLevel;

    //
    // HIERARCHICAL OVERRIDES
    //

    /// <summary>
    /// Namespace‑level minimum log levels.
    /// Example: { "MyApp.Services", SentinelLogLevel.Debug }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> NamespaceLevels { get; } = [];

    /// <summary>
    /// Class‑level minimum log levels.
    /// Example: { "MyApp.Services.OrderService", SentinelLogLevel.Warn }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> ClassLevels { get; } = [];

    /// <summary>
    /// Method‑level minimum log levels.
    /// Example: { "Divide", SentinelLogLevel.Debug }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> MethodLevels { get; } = [];


    //
    // INITIALIZATION LOGIC
    //

    public static void LoadFromFile(string path, bool monitorChanges = false)
    {
        if (!File.Exists(path))
            return;

        _configPath = path;

        var json = File.ReadAllText(path);
        InitializeFromJson(json);

        if (monitorChanges)
        {
            StartWatcher(path);
        }
    }

    public static void InitializeFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var root = JsonSerializer.Deserialize<AutoLoggerRootConfig>(json, options);
        if (root == null)
            return;

        var model = root.AutoLogger;
        if (model == null)
            return;

        if (model.Loggers != null)
        {
            foreach (var spec in model.Loggers)
            {
                var logger = SentinelLoggerFactory.Create(spec);
                SentinelLogger.Configure(logger);
            }
        }

        ApplyModel(model);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    private static void ApplyModel(AutoLoggerConfigModel model)
    {
        //
        // SYSTEM‑WIDE SETTINGS
        //

        MinimumLevel = model.MinimumLevel;

        //
        // NAMESPACE OVERRIDES
        //

        NamespaceLevels.Clear();
        if (model.NamespaceLevels != null)
        {
            foreach (var kvp in model.NamespaceLevels)
            {
                var entry = kvp.Value;
                if (entry != null)
                {
                    NamespaceLevels[kvp.Key] = new AutoLoggerLevel
                    {
                        Level = entry.Level,          // already an enum
                        Verbosity = entry.Verbosity == AutoLoggerConstants.VerbosityNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultVerbosity : MinimumLevel.Verbosity)
                            : entry.Verbosity,
                        WarningThresholdMs = entry.WarningThresholdMs == AutoLoggerConstants.WarningThresholdMsNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultWarningThresholdMs : MinimumLevel.WarningThresholdMs)
                            : entry.WarningThresholdMs
                    };
                }
            }
        }


        //
        // CLASS OVERRIDES
        //

        ClassLevels.Clear();
        if (model.ClassLevels != null)
        {
            foreach (var kvp in model.ClassLevels)
            {
                var entry = kvp.Value;
                if (entry != null)
                {
                    ClassLevels[kvp.Key] = new AutoLoggerLevel
                    {
                        Level = entry.Level,          // already an enum
                        Verbosity = entry.Verbosity == AutoLoggerConstants.VerbosityNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultVerbosity : MinimumLevel.Verbosity)
                            : entry.Verbosity,
                        WarningThresholdMs = entry.WarningThresholdMs == AutoLoggerConstants.WarningThresholdMsNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultWarningThresholdMs : MinimumLevel.WarningThresholdMs)
                            : entry.WarningThresholdMs
                    };
                }
            }
        }


        //
        // METHOD OVERRIDES
        //

        MethodLevels.Clear();
        if (model.MethodLevels != null)
        {
            foreach (var kvp in model.MethodLevels)
            {
                var entry = kvp.Value;
                if (entry != null)
                {
                    MethodLevels[kvp.Key] = new AutoLoggerLevel
                    {
                        Level = entry.Level,          // already an enum
                        Verbosity = entry.Verbosity == AutoLoggerConstants.VerbosityNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultVerbosity : MinimumLevel.Verbosity)
                            : entry.Verbosity,
                        WarningThresholdMs = entry.WarningThresholdMs == AutoLoggerConstants.WarningThresholdMsNotSet
                            ? (MinimumLevel is null ? AutoLoggerConstants.DefaultWarningThresholdMs : MinimumLevel.WarningThresholdMs)
                            : entry.WarningThresholdMs
                    };
                }
            }
        }
    }



    //
    // RESOLUTION LOGIC
    //

    /// <summary>
    /// Resolves the effective log level for a given event,
    /// applying method > class > namespace > system precedence.
    /// </summary>
    public static AutoLoggerLevel ResolveLevel(string fullName, string methodName)
    {
        // Method override
        if (MethodLevels.TryGetValue(methodName, out var methodLevel))
            return methodLevel;

        // Class override
        if (ClassLevels.TryGetValue(fullName, out var classLevel))
            return classLevel;

        // Namespace override
        var lastDot = fullName.LastIndexOf('.');
        if (lastDot > 0)
        {
            var ns = fullName[..lastDot];
            if (NamespaceLevels.TryGetValue(ns, out var nsLevel))
                return nsLevel;
        }

        // System default
        return MinimumLevel;
    }

    /// <summary>
    /// Determines whether a log event should be emitted,
    /// based on level and debug verbosity.
    /// </summary>
    public static bool ShouldLog(AutoLoggerLevel level, string fullName, string methodName)
    {
        var effectiveLevel = ResolveLevel(fullName, methodName);

        if (level.Level < effectiveLevel.Level)
            return false;

        if (level.Level == SentinelLogLevel.Debug && level.Verbosity > effectiveLevel.Verbosity)
            return false;

        return true;
    }

    public static bool IsLevel(SentinelLogLevel level)
    {
        return level <= MinimumLevel.Level;
    }

    public static bool IsLevel(AutoLoggerLevel level, string fullName, string methodName)
    {
        var effectiveLevel = ResolveLevel(fullName, methodName);

        return level.Level <= effectiveLevel.Level;
    }

    public static long WarningThresholdMs(string fullName, string methodName)
    {
        return ResolveLevel(fullName, methodName).WarningThresholdMs;
    }

    private static void StartWatcher(string path)
    {
        var directory = Path.GetDirectoryName(path);
        var file = Path.GetFileName(path);

        if (directory is null || file is null)
            return;

        _watcher?.Dispose();

        _watcher = new FileSystemWatcher(directory, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };

        _watcher.Changed += (_, __) => Reload();
        _watcher.Created += (_, __) => Reload();
        _watcher.Renamed += (_, __) => Reload();

        _watcher.EnableRaisingEvents = true;
    }

    private static void Reload()
    {
        try
        {
            if (_configPath is null || !File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            InitializeFromJson(json);
        }
        catch
        {
            // swallow errors — config reload should never crash the app
        }
    }
}

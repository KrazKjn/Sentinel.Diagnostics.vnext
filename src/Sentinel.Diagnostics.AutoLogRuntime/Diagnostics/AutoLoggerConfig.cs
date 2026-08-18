using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

public static class AutoLoggerConfig
{
    //
    // SYSTEM‑WIDE SETTINGS
    //

    /// <summary>
    /// Minimum log level for the entire application.
    /// Debug < Info < Warn < Error
    /// </summary>
    public static AutoLoggerLevel MinimumLevel { get; set; } = new AutoLoggerLevel { Level = SentinelLogLevel.Information, Verbosity = 0 };

    /// <summary>
    /// Duration threshold (ms) for warning logs.
    /// </summary>
    public static long WarningThresholdMs { get; set; } = 250;


    //
    // HIERARCHICAL OVERRIDES
    //

    /// <summary>
    /// Namespace‑level minimum log levels.
    /// Example: { "MyApp.Services", SentinelLogLevel.Debug }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> NamespaceLevels { get; } = new();

    /// <summary>
    /// Class‑level minimum log levels.
    /// Example: { "MyApp.Services.OrderService", SentinelLogLevel.Warn }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> ClassLevels { get; } = new();

    /// <summary>
    /// Method‑level minimum log levels.
    /// Example: { "Divide", SentinelLogLevel.Debug }
    /// </summary>
    public static Dictionary<string, AutoLoggerLevel> MethodLevels { get; } = new();


    //
    // INITIALIZATION LOGIC
    //

    public static void LoadFromFile(string path)
    {
        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        InitializeFromJson(json);
    }

    public static void InitializeFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var root = JsonSerializer.Deserialize<AutoLoggerRootConfig>(json, options);
        var model = root.AutoLogger;

        if (model == null)
            return;

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
        WarningThresholdMs = model.WarningThresholdMs;


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
                        Verbosity = entry.Verbosity == -1 ? MinimumLevel.Verbosity : entry.Verbosity
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
                        Verbosity = entry.Verbosity == -1 ? MinimumLevel.Verbosity : entry.Verbosity
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
                        Verbosity = entry.Verbosity == -1 ? MinimumLevel.Verbosity : entry.Verbosity
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
            var ns = fullName.Substring(0, lastDot);
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
    public static bool ShouldLog(SentinelLogLevel level, int verbosity, string fullName, string methodName)
    {
        var effectiveLevel = ResolveLevel(fullName, methodName);
        
        if (level < effectiveLevel.Level)
            return false;

        if (level == SentinelLogLevel.Debug && verbosity > effectiveLevel.Verbosity)
            return false;

        return true;
    }
}

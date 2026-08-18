using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class ProjectConfigurationLoader
{
    private const string SentinelConfigFileName = "sentinel.json";

    public static ProjectAutoLogOptions Load(ImmutableArray<AdditionalText> additionalFiles)
    {
        var sentinelFile = additionalFiles
            .FirstOrDefault(f =>
                string.Equals(Path.GetFileName(f.Path),
                SentinelConfigFileName,
                StringComparison.OrdinalIgnoreCase));

        if (sentinelFile is null)
            return new ProjectAutoLogOptions();

        var text = sentinelFile.GetText()?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return new ProjectAutoLogOptions();

        try
        {
            var json = JsonSerializer.Deserialize<SentinelProjectConfig>(text);
            if (json?.AutoLog is null)
                return new ProjectAutoLogOptions();

            return new ProjectAutoLogOptions
            {
                RawJson = text,
                Enabled = json.AutoLog.Enabled,
                AddUsing = json.AutoLog.AddUsing,
                AddTryCatch = json.AutoLog.AddTryCatch,
                LogParameters = json.AutoLog.LogParameters,
                LogDuration = json.AutoLog.LogDuration,
                Policy = json.AutoLog.Policy,
                Span = json.AutoLog.Span
            };
        }
        catch
        {
            return new ProjectAutoLogOptions();
        }
    }

    public static ProjectAutoLogOptions Load(string? jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return new ProjectAutoLogOptions();

        try
        {
            var json = JsonSerializer.Deserialize<SentinelProjectConfig>(jsonText);
            if (json?.AutoLog is null)
                return new ProjectAutoLogOptions();

            return new ProjectAutoLogOptions
            {
                Enabled = json.AutoLog.Enabled,
                AddUsing = json.AutoLog.AddUsing,
                AddTryCatch = json.AutoLog.AddTryCatch,
                LogParameters = json.AutoLog.LogParameters,
                LogDuration = json.AutoLog.LogDuration,
                Policy = json.AutoLog.Policy,
                Span = json.AutoLog.Span
            };
        }
        catch
        {
            return new ProjectAutoLogOptions();
        }
    }

    private sealed class SentinelProjectConfig
    {
        public AutoLogSection? AutoLog { get; set; }
    }

    private sealed class AutoLogSection
    {
        public bool? Enabled { get; set; }
        public bool? AddUsing { get; set; }
        public bool? AddTryCatch { get; set; }
        public bool? LogParameters { get; set; }
        public bool? LogDuration { get; set; }
        public string? Policy { get; set; }
        public string? Span { get; set; }
    }
}

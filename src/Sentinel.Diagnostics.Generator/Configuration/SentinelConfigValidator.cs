using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text.Json;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class SentinelConfigValidator
{
    public static void Validate(string? jsonText, SourceProductionContext context)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return;

        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (!root.TryGetProperty("AutoLog", out var autoLog))
            {
                Report(context, "SD0001", "Missing 'AutoLog' section.");
                return;
            }

            ValidateBoolean(autoLog, "Enabled", context);
            ValidateBoolean(autoLog, "AddUsing", context);
            ValidateBoolean(autoLog, "AddTryCatch", context);
            ValidateBoolean(autoLog, "LogParameters", context);
            ValidateBoolean(autoLog, "LogDuration", context);

            ValidateString(autoLog, "Policy", context);
            ValidateString(autoLog, "Span", context);

            ValidateNoUnknownProperties(autoLog, context);
        }
        catch (JsonException ex)
        {
            Report(context, "SD0000", $"Invalid JSON: {ex.Message}");
        }
    }

    private static void ValidateBoolean(JsonElement element, string name, SourceProductionContext context)
    {
        if (!element.TryGetProperty(name, out var prop))
            return;

        if (prop.ValueKind != JsonValueKind.True &&
            prop.ValueKind != JsonValueKind.False)
        {
            Report(context, "SD0002", $"Property '{name}' must be boolean.");
        }
    }

    private static void ValidateString(JsonElement element, string name, SourceProductionContext context)
    {
        if (!element.TryGetProperty(name, out var prop))
            return;

        if (prop.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(prop.GetString()))
        {
            Report(context, "SD0003", $"Property '{name}' must be a non-empty string.");
        }
    }

    private static void ValidateNoUnknownProperties(JsonElement element, SourceProductionContext context)
    {
        var allowed = new HashSet<string>
        {
            "Enabled", "AddUsing", "AddTryCatch",
            "LogParameters", "LogDuration",
            "Policy", "Span"
        };

        foreach (var prop in element.EnumerateObject())
        {
            if (!allowed.Contains(prop.Name))
            {
                Report(context, "SD0004", $"Unknown property '{prop.Name}'.");
            }
        }
    }

    private static void Report(SourceProductionContext context, string id, string message)
    {
        var descriptor = new DiagnosticDescriptor(
            id,
            "Sentinel Configuration Error",
            message,
            "SentinelConfig",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
    }
}
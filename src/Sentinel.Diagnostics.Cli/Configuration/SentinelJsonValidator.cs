using System.Text.Json;

namespace Sentinel.Diagnostics.Cli.Configuration;

public static class SentinelJsonValidator
{
    public static void Validate(string json, SentinelConfig config)
    {
        var diags = config.AutoLog!.Diagnostics;

        JsonDocument doc;

        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            diags.Add(new SentinelDiagnostic
            {
                Id = "SD0000",
                Message = $"Invalid JSON: {ex.Message}"
            });
            return;
        }

        if (!doc.RootElement.TryGetProperty("AutoLog", out var autoLog))
        {
            diags.Add(new SentinelDiagnostic
            {
                Id = "SD0004",
                Message = "Missing AutoLog section."
            });
            return;
        }

        ValidateBoolean(autoLog, "Enabled", diags);
        ValidateBoolean(autoLog, "AddUsing", diags);
        ValidateBoolean(autoLog, "AddTryCatch", diags);
        ValidateBoolean(autoLog, "LogParameters", diags);
        ValidateBoolean(autoLog, "LogDuration", diags);
    }

    private static void ValidateBoolean(JsonElement parent, string property, List<SentinelDiagnostic> diags)
    {
        if (!parent.TryGetProperty(property, out var value))
        {
            diags.Add(new SentinelDiagnostic
            {
                Id = "SD0004",
                Message = $"Unknown or missing property: {property}"
            });
            return;
        }

        if (value.ValueKind != JsonValueKind.True &&
            value.ValueKind != JsonValueKind.False)
        {
            diags.Add(new SentinelDiagnostic
            {
                Id = "SD0002",
                Message = $"{property} must be a boolean."
            });
        }
    }
}
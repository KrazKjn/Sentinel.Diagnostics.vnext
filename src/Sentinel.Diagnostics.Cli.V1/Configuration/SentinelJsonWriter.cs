using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sentinel.Diagnostics.Cli.Configuration;

public static class SentinelJsonWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task CreateDefaultAsync(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var defaultConfig = CreateDefaultConfig();

        var json = JsonSerializer.Serialize(defaultConfig, Options);
        await File.WriteAllTextAsync(path, json);
    }

    private static SentinelConfig CreateDefaultConfig()
    {
        return new SentinelConfig
        {
            AutoLog = new AutoLogSection
            {
                Enabled = true,
                AddUsing = true,
                AddTryCatch = true,
                LogParameters = true,
                LogDuration = true,
                RetryCount = 0,
                RetryDelayMilliseconds = 0
            }
        };
    }
}

public sealed class SentinelConfig
{
    public AutoLogSection? AutoLog { get; set; }
}

public sealed class AutoLogSection
{
    public bool Enabled { get; set; }
    public bool AddUsing { get; set; }
    public bool AddTryCatch { get; set; }
    public bool LogParameters { get; set; }
    public bool LogDuration { get; set; }

    public int RetryCount { get; set; }
    public int RetryDelayMilliseconds { get; set; }

    public List<SentinelDiagnostic> Diagnostics { get; set; } = [];
}

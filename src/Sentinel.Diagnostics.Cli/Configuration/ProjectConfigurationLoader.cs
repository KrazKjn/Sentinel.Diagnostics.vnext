using System.Text.Json;

namespace Sentinel.Diagnostics.Cli.Configuration;

public static class ProjectConfigurationLoader
{
    public static SentinelConfig LoadFromFile(string path)
    {
        if (!File.Exists(path))
            return new SentinelConfig();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SentinelConfig>(json)
               ?? new SentinelConfig();
    }
}

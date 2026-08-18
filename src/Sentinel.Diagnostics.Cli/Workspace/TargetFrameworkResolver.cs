namespace Sentinel.Diagnostics.Cli.Workspace;

internal static class TargetFrameworkResolver
{
    public static string ResolveFromOutputPath(string? outputFilePath)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            return "Unknown";
        }

        var directory =
            Path.GetDirectoryName(outputFilePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return "Unknown";
        }

        var directoryInfo =
            new DirectoryInfo(directory);

        var candidate =
            directoryInfo.Name;

        return IsTargetFramework(candidate)
            ? candidate
            : "Unknown";
    }

    private static bool IsTargetFramework(string value)
    {
        return value.StartsWith(
                   "net",
                   StringComparison.OrdinalIgnoreCase)
               &&
               value.Length > 3
               &&
               char.IsDigit(value[3]);
    }
}
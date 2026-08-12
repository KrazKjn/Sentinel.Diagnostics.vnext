using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class ProjectConfigurationLoader
{
    public static ProjectAutoLogOptions Load(Compilation compilation)
    {
        // Phase 4A: return hard-coded defaults
        // Phase 4C: read MSBuild AdditionalFiles
        // Phase 4D: parse sentinel.json
        // Phase 4E: validate and integrate

        return new ProjectAutoLogOptions
        {
            Enabled = null,
            AddUsing = null,
            AddTryCatch = null,
            LogParameters = null,
            LogDuration = null,
            Policy = null,
            Span = null
        };
    }
}

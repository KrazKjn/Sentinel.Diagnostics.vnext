using Sentinel.Diagnostics.Generator.Metadata;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class PolicyResolver
{
    public static string? ResolvePolicy(
        ValidatedMethodMetadata raw,
        ProjectAutoLogOptions project)
    {
        // Attribute override wins
        if (!string.IsNullOrWhiteSpace(raw.Attribute.Policy))
            return raw.Attribute.Policy;

        // Project-level default
        if (!string.IsNullOrWhiteSpace(project.Policy))
            return project.Policy;

        // Fallback
        return "default";
    }
}

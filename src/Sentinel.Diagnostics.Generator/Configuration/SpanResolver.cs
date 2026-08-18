using Sentinel.Diagnostics.Generator.Metadata;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class SpanResolver
{
    public static string? ResolveSpan(
        ValidatedMethodMetadata raw,
        ProjectAutoLogOptions project)
    {
        if (!string.IsNullOrWhiteSpace(raw.Attribute.Span))
            return raw.Attribute.Span;

        if (!string.IsNullOrWhiteSpace(project.Span))
            return project.Span;

        // Recommended fallback: method name
        return raw.MethodName;
    }
}

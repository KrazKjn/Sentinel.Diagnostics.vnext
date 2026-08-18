using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Models;

namespace Sentinel.Diagnostics.Generator.Configuration;

public static class EffectiveOptionsBuilder
{
    public static EffectiveAutoLogOptions Build(
        ValidatedMethodMetadata validated,
        ProjectAutoLogOptions project)
    {
        var attr = validated.Attribute;

        return new EffectiveAutoLogOptions
        {
            Enabled = attr.Enabled ?? project.Enabled ?? true,
            AddUsing = attr.AddUsing ?? project.AddUsing ?? true,
            AddTryCatch = attr.AddTryCatch ?? project.AddTryCatch ?? false,
            LogParameters = attr.LogParameters ?? project.LogParameters ?? true,
            LogDuration = attr.LogDuration ?? project.LogDuration ?? true,

            Policy = PolicyResolver.ResolvePolicy(validated, project) ?? "DefaultPolicy",
            Span = SpanResolver.ResolveSpan(validated, project) ?? string.Empty
        };
    }
}

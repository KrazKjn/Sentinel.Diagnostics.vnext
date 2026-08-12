using System.Collections.Generic;

namespace Sentinel.Diagnostics.Generator.Configuration;

internal static class ConfigurationResolver
{
    public static EffectiveAutoLogOptions Resolve(
        AutoLogConfiguration projectOptions,
        IReadOnlyList<AutoLogConfiguration> containingTypeOptions,
        AutoLogConfiguration? methodOptions,
        string methodName)
    {
        AutoLogConfiguration effective = projectOptions;

        foreach (AutoLogConfiguration typeOptions in containingTypeOptions)
        {
            effective = Merge(effective, typeOptions);
        }

        if (methodOptions is not null)
        {
            effective = Merge(effective, methodOptions);
        }

        return new EffectiveAutoLogOptions
        {
            Enabled = effective.Enabled ?? true,
            AddUsing = effective.AddUsing ?? true,
            AddTryCatch = effective.AddTryCatch ?? true,
            LogParameters = effective.LogParameters ?? true,
            LogDuration = effective.LogDuration ?? true,
            Policy = effective.Policy ?? "DefaultPolicy",
            Span = effective.Span ?? methodName
        };
    }

    private static AutoLogConfiguration Merge(
        AutoLogConfiguration parent,
        AutoLogConfiguration child)
    {
        return new AutoLogConfiguration(
            Enabled: child.Enabled ?? parent.Enabled,
            AddUsing: child.AddUsing ?? parent.AddUsing,
            AddTryCatch: child.AddTryCatch ?? parent.AddTryCatch,
            LogParameters: child.LogParameters ?? parent.LogParameters,
            LogDuration: child.LogDuration ?? parent.LogDuration,
            Policy: child.Policy ?? parent.Policy,
            Span: child.Span ?? parent.Span);
    }
}
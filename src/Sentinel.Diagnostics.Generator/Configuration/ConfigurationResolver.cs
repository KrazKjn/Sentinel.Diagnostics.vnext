using System.Collections.Generic;

namespace Sentinel.Diagnostics.Generator.Configuration;

internal static class ConfigurationResolver
{
    public static EffectiveAutoLogOptions Resolve(
        TypeAutoLogOptions projectOptions,
        IReadOnlyList<TypeAutoLogOptions> containingTypeOptions,
        TypeAutoLogOptions? methodOptions,
        string methodName)
    {
        TypeAutoLogOptions effective = projectOptions;

        foreach (TypeAutoLogOptions typeOptions in containingTypeOptions)
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

    private static TypeAutoLogOptions Merge(
        TypeAutoLogOptions parent,
        TypeAutoLogOptions child)
    {
        return new TypeAutoLogOptions(
            Enabled: child.Enabled ?? parent.Enabled,
            AddUsing: child.AddUsing ?? parent.AddUsing,
            AddTryCatch: child.AddTryCatch ?? parent.AddTryCatch,
            LogParameters: child.LogParameters ?? parent.LogParameters,
            LogDuration: child.LogDuration ?? parent.LogDuration,
            Policy: child.Policy ?? parent.Policy,
            Span: child.Span ?? parent.Span);
    }
}
using Sentinel.Diagnostics.Generator.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sentinel.Diagnostics.Generator.Configuration
{
    internal static class AutoLogOptionsResolver
    {
        public static EffectiveAutoLogOptions Resolve(
            AutoLogOptionsOverride defaults,
            AutoLogOptionsOverride? project,
            AutoLogOptionsOverride? containingType,
            AutoLogOptionsOverride? method)
        {
            // Default
            // → Project
            // → Class
            // → Method

            return new EffectiveAutoLogOptions
            {
                Enabled = method?.Enabled ?? containingType?.Enabled ?? project?.Enabled ?? defaults.Enabled ?? true,
                AddUsing = method?.AddUsing ?? containingType?.AddUsing ?? project?.AddUsing ?? defaults.AddUsing ?? true,
                AddTryCatch = method?.AddTryCatch ?? containingType?.AddTryCatch ?? project?.AddTryCatch ?? defaults.AddTryCatch ?? false,
                LogParameters = method?.LogParameters ?? containingType?.LogParameters ?? project?.LogParameters ?? defaults.LogParameters ?? true,
                LogDuration = method?.LogDuration ?? containingType?.LogDuration ?? project?.LogDuration ?? defaults.LogDuration ?? true,
                Policy = method?.Policy ?? containingType?.Policy ?? project?.Policy ?? defaults.Policy ?? "Default",
                Span = method?.Span ?? containingType?.Span ?? project?.Span ?? defaults.Span ?? "Default"
            };
        }
    }
}

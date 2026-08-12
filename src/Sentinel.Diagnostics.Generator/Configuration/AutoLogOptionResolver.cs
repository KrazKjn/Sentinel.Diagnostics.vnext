namespace Sentinel.Diagnostics.Generator.Configuration
{
    public static class AutoLogOptionResolver
    {
        public static EffectiveAutoLogOptions Resolve(
            MethodAutoLogOptions? method,
            TypeAutoLogOptions? type,
            ProjectAutoLogOptions? project,
            ProjectAutoLogOptions defaults)
        {
            return new EffectiveAutoLogOptions
            {
                Enabled =
                    method?.Enabled ??
                    type?.Enabled ??
                    project?.Enabled ??
                    defaults.Enabled ??
                    true,

                AddUsing =
                    method?.AddUsing ??
                    type?.AddUsing ??
                    project?.AddUsing ??
                    defaults.AddUsing ??
                    true,

                AddTryCatch =
                    method?.AddTryCatch ??
                    type?.AddTryCatch ??
                    project?.AddTryCatch ??
                    defaults.AddTryCatch ??
                    false,

                LogParameters =
                    method?.LogParameters ??
                    type?.LogParameters ??
                    project?.LogParameters ??
                    defaults.LogParameters ??
                    true,

                LogDuration =
                    method?.LogDuration ??
                    type?.LogDuration ??
                    project?.LogDuration ??
                    defaults.LogDuration ??
                    true,

                Policy =
                    method?.Policy ??
                    type?.Policy ??
                    project?.Policy ??
                    defaults.Policy ??
                    "Default",

                Span =
                    method?.Span ??
                    type?.Span ??
                    project?.Span ??
                    defaults.Span ??
                    "Default"
            };
        }
    }
}

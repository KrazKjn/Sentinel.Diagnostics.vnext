namespace Sentinel.Diagnostics.Generator.Configuration;

/// <summary>
/// Configuration values applicable to Sentinel Diagnostics instrumentation.
///
/// Nullable values indicate that the configuration level did not specify
/// a value and that resolution should continue to the parent configuration.
/// </summary>
public sealed record TypeAutoLogOptions(
    bool? Enabled = null,
    bool? AddUsing = null,
    bool? AddTryCatch = null,
    bool? LogParameters = null,
    bool? LogDuration = null,
    string? Policy = null,
    string? Span = null);
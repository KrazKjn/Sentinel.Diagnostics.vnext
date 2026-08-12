namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Raw values extracted directly from the <see cref="AutoLogAttribute"/>.
///
/// This record contains only the syntactic/semantic values provided by the
/// attribute itself. No resolution, merging, or defaulting occurs here.
///
/// The Metadata Analyzer produces this model, and the Metadata Builder
/// incorporates it into <see cref="SentinelMethodGenerationMetadata"/> for
/// later consumption by the vNext Instrumentation Engine.
/// </summary>
internal sealed record AutoLogAttributeData(
    string? Policy,
    string? Span,
    bool? Enabled,
    bool? AddUsing,
    bool? AddTryCatch,
    bool? LogParameters,
    bool? LogDuration);

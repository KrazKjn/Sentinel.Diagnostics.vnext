namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Raw values extracted from the AutoLog attribute itself.
/// </summary>
internal sealed record AutoLogAttributeData(
    string? Policy,
    string? Span);
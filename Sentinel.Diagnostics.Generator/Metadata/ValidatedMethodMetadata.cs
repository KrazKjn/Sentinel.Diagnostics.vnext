namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Represents the result of validating analyzed method metadata.
/// </summary>
internal sealed record ValidatedMethodMetadata(
    RawMethodMetadata Metadata);
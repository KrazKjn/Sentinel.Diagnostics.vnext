namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Represents the result of validating a method's analyzed metadata.
///
/// This model is produced after semantic analysis and before metadata
/// transformation. It indicates that the method has passed all structural
/// and attribute-based validation rules required for vNext instrumentation.
///
/// No transformation or rewriting logic occurs here.
/// </summary>
internal sealed record ValidatedMethodMetadata(
    RawMethodMetadata Metadata);
using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Finalized generator-side metadata describing a single method parameter.
///
/// This model is produced by <see cref="MetadataBuilder"/> and consumed by
/// the vNext Instrumentation Engine. It contains all information required
/// for method-body rewriting, sensitive-data filtering, and logging behavior.
///
/// Type information is represented using both simple and fully-qualified
/// names to support accurate code generation.
/// </summary>
public sealed record SentinelParameterGenerationMetadata(
    // Parameter identity.
    string Name,

    // Type information.
    string TypeName,
    string FullyQualifiedTypeName,

    // Parameter modifiers.
    RefKind RefKind,
    bool IsParams,
    bool IsNullable,

    // Attribute-derived configuration.
    bool IsSensitive,
    bool ShouldLog,

    // Default value information.
    bool HasExplicitDefaultValue,
    string? DefaultValueExpression
);

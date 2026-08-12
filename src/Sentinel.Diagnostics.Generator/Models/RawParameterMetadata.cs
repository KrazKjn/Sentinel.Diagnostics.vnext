using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Raw semantic metadata describing a single method parameter.
///
/// This is produced by <see cref="MetadataAnalyzer"/> and consumed by
/// <see cref="MetadataBuilder"/>. No transformation or instrumentation
/// logic occurs here.
///
/// All Roslyn symbols have already been resolved. This record represents
/// the generator-side view of a parameter before it is transformed into
/// <see cref="SentinelParameterGenerationMetadata"/> for use by the
/// vNext Instrumentation Engine.
/// </summary>
public sealed record RawParameterMetadata(
    // Parameter identity.
    string Name,

    // Type information.
    string TypeName,
    string FullyQualifiedTypeName,

    // Parameter modifiers.
    RefKind RefKind,
    bool IsParams,

    // Nullability context.
    bool IsNullable,

    // Attribute-derived configuration.
    bool IsSensitive,
    bool ShouldLog,

    // Default value information.
    bool HasExplicitDefaultValue,
    string? DefaultValueExpression
);

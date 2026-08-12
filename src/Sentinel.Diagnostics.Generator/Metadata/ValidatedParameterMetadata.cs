using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Represents fully validated, normalized parameter metadata used by the
/// vNext instrumentation engine. All raw semantic fields have been removed.
/// </summary>
public sealed record ValidatedParameterMetadata(
    string Name,
    string FullyQualifiedTypeName,
    RefKind RefKind,
    bool IsParams,
    bool IsNullable,
    bool HasExplicitDefaultValue,
    string? DefaultValueExpression,
    bool IsSensitive,
    bool ShouldLog);
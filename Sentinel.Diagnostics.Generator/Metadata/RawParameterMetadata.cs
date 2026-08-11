using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Raw metadata describing a method parameter.
/// </summary>
public sealed record RawParameterMetadata(
    string Name,
    string ParameterType,
    bool IsSensitive,
    bool ShouldLog,
    // Parameter declaration/invocation characteristics.
    RefKind RefKind,
    bool IsParams,
    bool HasExplicitDefaultValue,
    string? DefaultValueExpression);
using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Finalized parameter metadata used by the Sentinel Diagnostics
/// source emitters.
///
/// Type information remains represented as a fully qualified type-name
/// string so that the emitter can generate the appropriate C# source.
/// </summary>
public sealed record SentinelParameterGenerationMetadata(
    string Name,
    string ParameterType,
    bool IsSensitive,
    bool ShouldLog,
    RefKind RefKind,
    bool IsParams,
    bool HasExplicitDefaultValue,
    string? DefaultValueExpression);

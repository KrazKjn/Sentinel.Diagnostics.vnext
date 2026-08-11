using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Finalized method metadata used by the Sentinel Diagnostics source emitters.
///
/// This is a generator-side representation and is intentionally separate
/// from Sentinel.Diagnostics.Core.Metadata.SentinelMethodMetadata.
///
/// Type information remains represented as fully qualified type-name
/// strings because the generator produces source code rather than runtime
/// System.Type instances.
/// </summary>
public sealed record SentinelMethodGenerationMetadata(
    string MethodName,
    Location MethodLocation,
    string FullyQualifiedMethodName,

    // Declaring type information.
    string DeclaringNamespace,
    string DeclaringTypeName,
    string FullyQualifiedDeclaringTypeName,
    ImmutableArray<ContainingTypeMetadata> ContainingTypes,

    // Method characteristics.
    string ReturnType,
    string SpanName,
    string? PolicyName,
    bool IsAsync,
    bool IsStatic,
    bool HasCancellationToken,

    Accessibility MethodAccessibility,
    Accessibility DeclaringTypeAccessibility,

    // Parameters.
    ImmutableArray<SentinelParameterGenerationMetadata> Parameters);
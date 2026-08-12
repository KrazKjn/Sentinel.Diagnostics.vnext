using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Models;

/// <summary>
/// Raw semantic metadata extracted from a method decorated with
/// the Sentinel Diagnostics <see cref="AutoLogAttribute"/>.
///
/// This is the generator-side intermediate representation produced
/// by <see cref="MetadataAnalyzer"/> and consumed by
/// <see cref="MetadataBuilder"/>.
///
/// All Roslyn symbols have already been resolved. No transformation
/// or policy/span resolution occurs here. The Metadata Builder
/// converts this raw semantic data into the finalized
/// <see cref="SentinelMethodGenerationMetadata"/> model used by the
/// vNext Instrumentation Engine.
/// </summary>
public sealed record RawMethodMetadata(
    // Method identity.
    string MethodName,
    Location MethodLocation,
    string FullyQualifiedMethodName,

    // Declaring type information.
    string DeclaringNamespace,
    string DeclaringTypeName,
    string FullyQualifiedDeclaringTypeName,

    ImmutableArray<ContainingTypeMetadata> ContainingTypes,

    ImmutableArray<RawParameterMetadata> Parameters,

    // Raw attribute values (may be null, invalid, missing)
    // Attribute-derived configuration.
    EffectiveAutoLogOptions Options,

    // Method characteristics.
    string ReturnTypeName,
    string FullyQualifiedReturnTypeName,

    bool IsAsync,
    bool IsIterator,
    bool IsStatic,
    bool HasCancellationToken,

    bool IsGenericMethod,
    ImmutableArray<string> GenericTypeParameters,


    // Accessibility information.
    Accessibility MethodAccessibility,
    Accessibility DeclaringTypeAccessibility);

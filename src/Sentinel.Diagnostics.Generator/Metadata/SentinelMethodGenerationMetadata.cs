using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Finalized generator-side metadata describing a method eligible for
/// Sentinel Diagnostics vNext instrumentation.
///
/// This model is produced by <see cref="MetadataBuilder"/> and consumed by
/// the vNext Instrumentation Engine. It contains all semantic information
/// required for method-body rewriting, logging behavior, sensitive-data
/// filtering, and policy/span integration.
///
/// Type information is represented using fully qualified type-name strings
/// because the generator operates at compile time and does not use runtime
/// System.Type instances.
/// </summary>
public sealed record SentinelMethodGenerationMetadata(
    // Method identity.
    string MethodName,
    Location MethodLocation,
    string FullyQualifiedMethodName,

    // Declaring type information.
    string DeclaringNamespace,
    string DeclaringTypeName,
    string FullyQualifiedDeclaringTypeName,
    ImmutableArray<ContainingTypeMetadata> ContainingTypes,

    // Method characteristics.
    string ReturnTypeName,
    string FullyQualifiedReturnTypeName,
    bool IsAsync,
    bool IsIterator,
    bool IsStatic,
    bool HasCancellationToken,
    bool IsGenericMethod,
    ImmutableArray<string> GenericTypeParameters,

    // Attribute-derived configuration.
    EffectiveAutoLogOptions Options,

    // Accessibility information.
    Accessibility MethodAccessibility,
    Accessibility DeclaringTypeAccessibility,

    // Parameters.
    ImmutableArray<SentinelParameterGenerationMetadata> Parameters);

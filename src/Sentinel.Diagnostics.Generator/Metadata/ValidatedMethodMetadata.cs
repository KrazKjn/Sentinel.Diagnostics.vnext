using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Represents fully validated, normalized, and generator‑ready metadata
/// for a method that has passed Phase‑3 validation rules.
/// 
/// This model contains *only* the information required by the vNext
/// instrumentation engine. All raw semantic data, syntax-only fields,
/// accessibility, and return-type information have been removed.
/// </summary>
public sealed record ValidatedMethodMetadata(
    // Method identity
    string MethodName,
    Location MethodLocation,
    string FullyQualifiedMethodName,

    // Declaring type information
    string DeclaringNamespace,
    string DeclaringTypeName,
    string FullyQualifiedDeclaringTypeName,
    ImmutableArray<ContainingTypeMetadata> ContainingTypes,

    // Validated AutoLog configuration
    string SpanName,      // normalized, never null or empty
    string PolicyName,    // normalized, never null or empty

    // Method characteristics
    string ReturnTypeName,
    string FullyQualifiedReturnTypeName,
    bool IsAsync,
    bool IsIterator,
    bool IsStatic,
    bool HasCancellationToken,

    // Generic method information
    ImmutableArray<string> GenericTypeParameters,

    // Accessibility information.
    Accessibility MethodAccessibility,
    Accessibility DeclaringTypeAccessibility,

    // Validated parameter metadata
    ImmutableArray<ValidatedParameterMetadata> Parameters);

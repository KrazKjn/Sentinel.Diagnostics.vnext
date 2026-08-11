using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Raw metadata extracted from an AutoLog-decorated method.
///
/// This is the generator-side intermediate representation produced by
/// the Metadata Analyzer and consumed by the Metadata Builder.
///
/// All semantic information has already been resolved from Roslyn symbols.
/// The Metadata Builder performs transformation only.
/// </summary>
public sealed record RawMethodMetadata(
    string MethodName,
    Location MethodLocation,
    string FullyQualifiedMethodName,

    // Declaring type information.
    string DeclaringNamespace,
    string DeclaringTypeName,
    string FullyQualifiedDeclaringTypeName,

    ImmutableArray<ContainingTypeMetadata> ContainingTypes,
    ImmutableArray<RawParameterMetadata> Parameters,

    // Method characteristics.
    string ReturnType,
    bool IsAsync,
    bool IsStatic,
    bool HasCancellationToken,

    // Diagnostic configuration.
    string SpanName,
    string? PolicyName,

    Accessibility MethodAccessibility,
    Accessibility DeclaringTypeAccessibility);

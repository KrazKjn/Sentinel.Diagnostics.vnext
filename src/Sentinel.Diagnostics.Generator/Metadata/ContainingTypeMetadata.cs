using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Describes a containing type in the method's declaring hierarchy.
///
/// This generator-side metadata is used to preserve the semantic structure
/// of the original type hierarchy for vNext instrumentation. It does not
/// participate in runtime metadata and does not reconstruct source code.
/// </summary>
public sealed record ContainingTypeMetadata(
    // Type identity.
    string Name,
    string FullyQualifiedName,
    string Namespace,

    // Semantic characteristics.
    Accessibility Accessibility,
    TypeKind TypeKind,

    // Generic type parameters (if any).
    ImmutableArray<string> TypeParameters);
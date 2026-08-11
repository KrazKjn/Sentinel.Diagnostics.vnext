using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sentinel.Diagnostics.Generator.Metadata;

/// <summary>
/// Describes a containing type required by the source generator when
/// reconstructing the original type hierarchy in generated source.
///
/// This is generator-side metadata and is not part of the Sentinel
/// Diagnostics runtime metadata model.
/// </summary>
public sealed record ContainingTypeGenerationMetadata(
    string Name,
    string FullyQualifiedName,
    string Namespace,
    Accessibility Accessibility,
    TypeKind TypeKind,
    bool IsPartial,
    bool IsStatic,
    bool IsReadOnly,
    bool IsRecord,
    ImmutableArray<string> TypeParameters,
    ImmutableArray<string> TypeParameterConstraints);
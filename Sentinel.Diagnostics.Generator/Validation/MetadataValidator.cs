using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Compatibility;
//using Sentinel.Diagnostics.Generator.Diagnostics;
using Sentinel.Diagnostics.Generator.Metadata;
using System;

namespace Sentinel.Diagnostics.Generator.Validation;

/// <summary>
/// Validates analyzed Sentinel Diagnostics metadata before it is passed
/// to the metadata builder and source emitters.
///
/// This class does not perform semantic analysis. The metadata it receives
/// has already been resolved by MetadataAnalyzer.
/// </summary>
internal static class MetadataValidator
{
    /// <summary>
    /// Validates metadata required for source generation.
    /// </summary>
    /// <param name="context">
    /// The source production context used to report diagnostics.
    /// </param>
    /// <param name="metadata">
    /// Raw metadata produced by MetadataAnalyzer.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the method can safely proceed through
    /// the source-generation pipeline; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Validate(
        SourceProductionContext context,
        RawMethodMetadata metadata)
    {
        Guard.NotNull(metadata, nameof(metadata));

        return ValidateContainingTypes(
            context,
            metadata);
    }

    /// <summary>
    /// Ensures that every containing type is declared partial.
    ///
    /// All containing types must be partial because generated members
    /// are emitted into the original type hierarchy.
    /// </summary>
    private static bool ValidateContainingTypes(
        SourceProductionContext context,
        RawMethodMetadata metadata)
    {
        foreach (ContainingTypeMetadata type
                 in metadata.ContainingTypes)
        {
            if (type.IsPartial)
            {
                continue;
            }

            //Diagnostic diagnostic =
            //    Diagnostic.Create(
            //        SentinelGeneratorDiagnostics
            //            .ContainingTypeMustBePartial,
            //        location: null,
            //        type.Name,
            //        metadata.MethodName);

            //context.ReportDiagnostic(diagnostic);

            return false;
        }

        return true;
    }
}
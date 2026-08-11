using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Compatibility;
using Sentinel.Diagnostics.Generator.Metadata;

namespace Sentinel.Diagnostics.Generator.Validation;

/// <summary>
/// Performs structural and attribute-based validation on analyzed
/// Sentinel Diagnostics metadata before it is transformed by the
/// <see cref="MetadataBuilder"/>.
///
/// This validator does not perform semantic analysis. All semantic
/// information has already been resolved by <see cref="MetadataAnalyzer"/>.
/// </summary>
internal static class MetadataValidator
{
    /// <summary>
    /// Validates metadata required for vNext instrumentation.
    /// </summary>
    public static bool Validate(
        SourceProductionContext context,
        RawMethodMetadata metadata)
    {
        Guard.NotNull(metadata, nameof(metadata));

        // Phase 3 will introduce new validation rules:
        // - AutoLog attribute correctness
        // - Unsupported method kinds (iterator, async iterator, etc.)
        // - Invalid parameter configurations
        // - Policy/span validation
        // - Sensitive parameter validation
        // - CancellationToken usage rules

        return true;
    }
}

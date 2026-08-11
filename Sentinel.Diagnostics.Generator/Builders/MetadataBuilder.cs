using Sentinel.Diagnostics.Generator.Compatibility;
using Sentinel.Diagnostics.Generator.Metadata;
using System;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Builders;

/// <summary>
/// Converts raw metadata produced by the Metadata Analyzer into finalized
/// metadata used by the Sentinel Diagnostics source emitters.
///
/// The Metadata Builder performs transformation only. It does not:
/// - Perform semantic analysis.
/// - Access Roslyn syntax nodes or symbols.
/// - Resolve System.Type instances.
/// - Generate source code.
/// - Generate wrappers.
/// - Generate policies.
///
/// Type information remains represented as fully qualified type-name strings.
/// The emitters are responsible for converting those names into generated
/// C# expressions such as typeof(global::MyApplication.Customer).
/// </summary>
public sealed class MetadataBuilder
{
    /// <summary>
    /// Builds finalized generation metadata from raw analyzer metadata.
    /// </summary>
    /// <param name="rawMetadata">
    /// Raw metadata produced by the Metadata Analyzer.
    /// </param>
    /// <returns>
    /// Finalized metadata consumed by the source emitters.
    /// </returns>
    public static SentinelMethodGenerationMetadata Build(
        RawMethodMetadata rawMetadata)
    {
        Guard.NotNull(rawMetadata, nameof(rawMetadata));

        return new SentinelMethodGenerationMetadata(
            MethodName: rawMetadata.MethodName,
            MethodLocation: rawMetadata.MethodLocation,
            FullyQualifiedMethodName: rawMetadata.FullyQualifiedMethodName,

            DeclaringNamespace: rawMetadata.DeclaringNamespace,
            DeclaringTypeName: rawMetadata.DeclaringTypeName,
            FullyQualifiedDeclaringTypeName:
                rawMetadata.FullyQualifiedDeclaringTypeName,

            ContainingTypes: rawMetadata.ContainingTypes,
            Parameters: BuildParameters(rawMetadata.Parameters),

            ReturnType: rawMetadata.ReturnType,
            SpanName: rawMetadata.SpanName,
            PolicyName: rawMetadata.PolicyName,

            IsAsync: rawMetadata.IsAsync,
            IsStatic: rawMetadata.IsStatic,
            HasCancellationToken: rawMetadata.HasCancellationToken,

            MethodAccessibility: rawMetadata.MethodAccessibility,
            DeclaringTypeAccessibility: rawMetadata.DeclaringTypeAccessibility);
    }

    /// <summary>
    /// Converts raw parameter metadata into finalized generation metadata.
    /// </summary>
    private static ImmutableArray<SentinelParameterGenerationMetadata>
        BuildParameters(
            ImmutableArray<RawParameterMetadata> rawParameters)
    {
        if (rawParameters.IsDefaultOrEmpty)
        {
            return ImmutableArray<SentinelParameterGenerationMetadata>.Empty;
        }

        var builder =
            ImmutableArray.CreateBuilder<SentinelParameterGenerationMetadata>(
                rawParameters.Length);

        foreach (RawParameterMetadata parameter in rawParameters)
        {
            builder.Add(
                new SentinelParameterGenerationMetadata(
                    Name: parameter.Name,
                    ParameterType: parameter.ParameterType,
                    IsSensitive: parameter.IsSensitive,
                    ShouldLog: parameter.ShouldLog,
                    RefKind: parameter.RefKind,
                    IsParams: parameter.IsParams,
                    HasExplicitDefaultValue:
                        parameter.HasExplicitDefaultValue,
                    DefaultValueExpression:
                        parameter.DefaultValueExpression));
        }

        return builder.ToImmutable();
    }
}

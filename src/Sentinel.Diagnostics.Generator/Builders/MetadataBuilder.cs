using Sentinel.Diagnostics.Generator.Compatibility;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Builders;

/// <summary>
/// Transforms raw semantic metadata produced by <see cref="MetadataAnalyzer"/>
/// into finalized generator-side metadata consumed by the vNext
/// Instrumentation Engine.
///
/// The Metadata Builder performs transformation only. It does not:
/// - Perform semantic analysis.
/// - Access Roslyn syntax nodes or symbols.
/// - Resolve System.Type instances.
/// - Generate source code.
/// - Generate policies.
/// - Reconstruct type declarations.
///
/// All type information remains represented as fully qualified type-name
/// strings because the generator operates at compile time and does not use
/// runtime reflection.
/// </summary>
public sealed class MetadataBuilder
{
    /// <summary>
    /// Builds finalized generation metadata from raw analyzer metadata.
    /// </summary>
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
            FullyQualifiedDeclaringTypeName: rawMetadata.FullyQualifiedDeclaringTypeName,

            ContainingTypes: rawMetadata.ContainingTypes,
            Parameters: BuildParameters(rawMetadata.Parameters),

            ReturnTypeName: rawMetadata.ReturnTypeName,
            FullyQualifiedReturnTypeName: rawMetadata.FullyQualifiedReturnTypeName,

            SpanName: rawMetadata.SpanName,
            PolicyName: rawMetadata.PolicyName,

            IsAsync: rawMetadata.IsAsync,
            IsIterator: rawMetadata.IsIterator,
            IsStatic: rawMetadata.IsStatic,
            IsGenericMethod: rawMetadata.IsGenericMethod,
            GenericTypeParameters: rawMetadata.GenericTypeParameters,
            HasCancellationToken: rawMetadata.HasCancellationToken,

            MethodAccessibility: rawMetadata.MethodAccessibility,
            DeclaringTypeAccessibility: rawMetadata.DeclaringTypeAccessibility);
    }

    /// <summary>
    /// Converts raw parameter metadata into finalized generation metadata.
    /// </summary>
    private static ImmutableArray<SentinelParameterGenerationMetadata> BuildParameters(
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
                    TypeName: parameter.TypeName,
                    FullyQualifiedTypeName: parameter.FullyQualifiedTypeName,
                    RefKind: parameter.RefKind,
                    IsParams: parameter.IsParams,
                    IsNullable: parameter.IsNullable,
                    IsSensitive: parameter.IsSensitive,
                    ShouldLog: parameter.ShouldLog,
                    HasExplicitDefaultValue: parameter.HasExplicitDefaultValue,
                    DefaultValueExpression: parameter.DefaultValueExpression));
        }

        return builder.ToImmutable();
    }
}
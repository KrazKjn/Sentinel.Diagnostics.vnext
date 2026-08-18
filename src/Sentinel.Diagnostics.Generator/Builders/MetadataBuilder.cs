using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Compatibility;
using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Models;
using System.Collections.Immutable;
using System.Linq;

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

            Options: rawMetadata.Options,

            IsAsync: rawMetadata.IsAsync,
            IsIterator: rawMetadata.IsIterator,
            IsStatic: rawMetadata.IsStatic,
            IsGenericMethod: rawMetadata.IsGenericMethod,
            GenericTypeParameters: rawMetadata.GenericTypeParameters,
            HasCancellationToken: rawMetadata.HasCancellationToken,

            MethodAccessibility: rawMetadata.MethodAccessibility,
            DeclaringTypeAccessibility: rawMetadata.DeclaringTypeAccessibility);
    }

    public static SentinelMethodGenerationMetadata Build(
        ValidatedMethodMetadata validated,
         EffectiveAutoLogOptions effective)
    {
        Guard.NotNull(validated, nameof(validated));

        var parameters =
            validated.Parameters
                .Select(p => new SentinelParameterGenerationMetadata(
                    Name: p.Name,
                    TypeName: p.FullyQualifiedTypeName.Split('.').Last(),
                    FullyQualifiedTypeName: p.FullyQualifiedTypeName,
                    IsSensitive: p.IsSensitive,
                    IsNullable: p.IsNullable,
                    ShouldLog: p.ShouldLog,
                    RefKind: p.RefKind,
                    IsParams: p.IsParams,
                    HasExplicitDefaultValue: p.HasExplicitDefaultValue,
                    DefaultValueExpression: p.DefaultValueExpression
                ))
                .ToImmutableArray();

        // Build your final metadata from validated input
        return new SentinelMethodGenerationMetadata(
            MethodName: validated.MethodName,
            MethodLocation: validated.MethodLocation,
            FullyQualifiedMethodName: validated.FullyQualifiedMethodName,
            DeclaringNamespace: validated.DeclaringNamespace,
            DeclaringTypeName: validated.DeclaringTypeName,
            FullyQualifiedDeclaringTypeName: validated.FullyQualifiedDeclaringTypeName,
            ContainingTypes: validated.ContainingTypes,
            Options: effective,
            ReturnTypeName: validated.ReturnTypeName,
            FullyQualifiedReturnTypeName: validated.FullyQualifiedReturnTypeName,
            IsAsync: validated.IsAsync,
            IsIterator: validated.IsIterator,
            IsStatic: validated.IsStatic,
            IsGenericMethod: validated.GenericTypeParameters.Length > 0,
            HasCancellationToken: validated.HasCancellationToken,
            GenericTypeParameters: validated.GenericTypeParameters,
            MethodAccessibility: validated.MethodAccessibility,
            DeclaringTypeAccessibility: validated.DeclaringTypeAccessibility,
            Parameters: parameters
        );
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
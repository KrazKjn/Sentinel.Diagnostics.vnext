using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Builders;
using Sentinel.Diagnostics.Generator.Emitters;
using Sentinel.Diagnostics.Generator.Metadata;
using System;
using System.Linq;

namespace Sentinel.Diagnostics.Generator;

/// <summary>
/// Sentinel Diagnostics incremental source generator.
///
/// The generator orchestrates the following pipeline:
///
/// Syntax Discovery
///     ↓
/// MethodDeclarationSyntax
///     ↓
/// Metadata Analysis
///     ↓
/// RawMethodMetadata
///     ↓
/// Metadata Building
///     ↓
/// SentinelMethodGenerationMetadata
///     ├──→ PolicyEmitter
///     └──→ WrapperEmitter
///
/// This class is responsible only for composing the incremental
/// generator pipeline. Semantic analysis, metadata construction,
/// and source generation are delegated to their respective
/// components.
/// </summary>
[Generator]
public sealed class SentinelDiagnosticsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the Sentinel Diagnostics incremental generator.
    /// </summary>
    /// <param name="context">
    /// The incremental generator initialization context.
    /// </param>
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        /*
         * ================================================================
         * Stage 1
         * Syntax Discovery
         * ================================================================
         *
         * SyntaxProvider performs syntax-only filtering.
         *
         * It identifies MethodDeclarationSyntax nodes that contain
         * a syntactically named AutoLog attribute.
         *
         * No semantic analysis is performed at this stage.
         */
        IncrementalValuesProvider<MethodDeclarationSyntax>
            methodDeclarations =
                context.SyntaxProvider.CreateSyntaxProvider(
                    static (node, _) =>
                        IsCandidateMethod(node),

                    static (syntaxContext, _) =>
                        (MethodDeclarationSyntax)syntaxContext.Node);

        /*
         * ================================================================
         * Stage 2
         * Semantic Metadata Analysis
         * ================================================================
         *
         * Each candidate method is combined with the Compilation so
         * MetadataAnalyzer can obtain the appropriate SemanticModel.
         *
         * MetadataAnalyzer is responsible for resolving:
         *
         * - IMethodSymbol
         * - AutoLogAttribute
         * - SensitiveAttribute
         * - method name
         * - fully qualified method name
         * - declaring namespace
         * - declaring type
         * - fully qualified declaring type
         * - return type
         * - async state
         * - static state
         * - method accessibility
         * - declaring type accessibility
         * - CancellationToken information
         * - parameter types
         * - parameter sensitivity
         * - parameter logging state
         * - parameter RefKind
         * - parameter IsParams
         * - policy name
         * - span name
         *
         * This class does not perform any of that analysis directly.
         */
        IncrementalValuesProvider<RawMethodMetadata?>
            analyzedMethods =
                methodDeclarations
                    .Combine(context.CompilationProvider)
                    .Select(
                        static (pair, _) =>
                        {
                            (
                                MethodDeclarationSyntax methodDeclaration,
                                Compilation compilation) = pair;

                            SemanticModel semanticModel =
                                compilation.GetSemanticModel(
                                    methodDeclaration.SyntaxTree);

                            var analyzer =
                                new MetadataAnalyzer(compilation);

                            return analyzer.Analyze(
                                semanticModel,
                                methodDeclaration);
                        });

        /*
         * Remove methods that could not be analyzed successfully.
         *
         * MetadataAnalyzer returns null when:
         *
         * - the method symbol cannot be resolved
         * - the AutoLog attribute cannot be resolved
         * - required semantic information is unavailable
         */
        IncrementalValuesProvider<RawMethodMetadata>
            validAnalyzedMethods =
                analyzedMethods
                    .Where(
                        static metadata =>
                            metadata is not null)
                    .Select(
                        static (metadata, _) =>
                            metadata!);

        /*
         * ================================================================
         * Stage 3
         * Metadata Building
         * ================================================================
         *
         * MetadataBuilder transforms RawMethodMetadata into finalized
         * generator-side metadata.
         *
         * MetadataBuilder performs no semantic analysis and generates
         * no source code.
         *
         * The resulting SentinelMethodGenerationMetadata contains all
         * information required by the source emitters, including:
         *
         * - method identity
         * - declaring type information
         * - return type
         * - policy
         * - span
         * - IsAsync
         * - IsStatic
         * - HasCancellationToken
         * - method accessibility
         * - declaring type accessibility
         * - parameter metadata
         * - RefKind
         * - IsParams
         */
        IncrementalValuesProvider<SentinelMethodGenerationMetadata>
            methodMetadataProvider =
                validAnalyzedMethods.Select(
                    static (rawMetadata, _) =>
                    {
                        //var builder =
                        //    new MetadataBuilder();

                        return MetadataBuilder.Build(
                            rawMetadata);
                    });
        // =================================================================
        // Stage 4a
        // Diagnostics Infrastructure
        // =================================================================

        context.RegisterSourceOutput(
            methodMetadataProvider.Collect(),
            static (productionContext, methods) =>
            {
                DiagnosticsEmitter.Emit(
                    productionContext,
                    methods);
            });

        /*
         * ================================================================
         * Stage 4b
         * Policy Emission
         * ================================================================
         *
         * PolicyEmitter generates one policy class per unique policy.
         *
         * Because policy generation requires the complete collection of
         * methods, the metadata provider is collected before emission.
         *
         * PolicyEmitter is responsible only for source generation.
         * It does not perform semantic analysis or metadata extraction.
         */
        context.RegisterSourceOutput(
            methodMetadataProvider.Collect(),
            static (productionContext, methods) =>
            {
                PolicyEmitter.Emit(
                    productionContext,
                    methods);
            });

        /*
         * ================================================================
         * Stage 4c
         * Wrapper Emission
         * ================================================================
         *
         * WrapperEmitter processes each finalized method independently.
         *
         * It receives the completed metadata contract and is responsible
         * only for generating the wrapper source code.
         *
         * WrapperEmitter uses:
         *
         * - IsStatic to determine static versus instance invocation
         * - FullyQualifiedDeclaringTypeName for type references
         * - RefKind for ref/out/in parameters
         * - IsParams for params declarations
         * - HasCancellationToken for token pass-through
         * - IsAsync for async/await generation
         * - ReturnType for return handling
         * - IsSensitive and ShouldLog for parameter logging
         * - PolicyName and SpanName for diagnostic behavior
         */

        //context.RegisterSourceOutput(
        //    methodMetadataProvider,
        //    static (productionContext, methodMetadata) =>
        //    {
        //        WrapperEmitter.Emit(
        //            productionContext,
        //            methodMetadata);
        //    });
    }

    /// <summary>
    /// Performs syntax-only filtering for potential AutoLog methods.
    ///
    /// This method does not use a SemanticModel and does not resolve
    /// the AutoLog attribute symbol. Semantic resolution is performed
    /// by MetadataAnalyzer.
    /// </summary>
    private static bool IsCandidateMethod(
        SyntaxNode node)
    {
        if (node is not MethodDeclarationSyntax method)
        {
            return false;
        }

        return method.AttributeLists
            .SelectMany(
                static list =>
                    list.Attributes)
            .Any(
                static attribute =>
                    IsAutoLogAttributeSyntax(attribute));
    }

    /// <summary>
    /// Determines whether an attribute syntax node could represent
    /// the Sentinel Diagnostics AutoLog attribute.
    ///
    /// This method performs syntax-only filtering. It deliberately
    /// does not resolve the attribute symbol.
    /// </summary>
    private static bool IsAutoLogAttributeSyntax(
        AttributeSyntax attribute)
    {
        string name =
            attribute.Name.ToString();

        return name.Equals(
                   "AutoLog",
                   StringComparison.Ordinal) ||
               name.Equals(
                   "AutoLogAttribute",
                   StringComparison.Ordinal) ||
               name.EndsWith(
                   ".AutoLog",
                   StringComparison.Ordinal) ||
               name.EndsWith(
                   ".AutoLogAttribute",
                   StringComparison.Ordinal);
    }
}
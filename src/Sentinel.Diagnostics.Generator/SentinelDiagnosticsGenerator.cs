using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Builders;
using Sentinel.Diagnostics.Generator.Metadata;
using System;
using System.Linq;

namespace Sentinel.Diagnostics.Generator;

/// <summary>
/// Sentinel Diagnostics incremental generator.
///
/// The generator orchestrates the following pipeline:
///
///   Syntax Discovery
///       ↓
///   MethodDeclarationSyntax
///       ↓
///   Semantic Metadata Analysis
///       ↓
///   RawMethodMetadata
///       ↓
///   Metadata Building
///       ↓
///   SentinelMethodGenerationMetadata
///       └──→ consumed by the vNext Instrumentation Engine
///
/// This class composes the incremental generator pipeline. All semantic
/// analysis and metadata construction are delegated to their respective
/// components. The generator does not emit source code; it produces
/// metadata consumed by the instrumentation layer.
/// </summary>
[Generator]
public sealed class SentinelDiagnosticsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the Sentinel Diagnostics incremental generator.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        /*
         * ================================================================
         * Stage 1 — Syntax Discovery
         * ================================================================
         *
         * SyntaxProvider performs syntax-only filtering.
         *
         * It identifies MethodDeclarationSyntax nodes that contain an
         * attribute syntactically named AutoLog or AutoLogAttribute.
         *
         * No semantic analysis is performed at this stage.
         */
        IncrementalValuesProvider<MethodDeclarationSyntax> methodDeclarations =
            context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsCandidateMethod(node),
                static (syntaxContext, _) => (MethodDeclarationSyntax)syntaxContext.Node);

        /*
         * ================================================================
         * Stage 2 — Semantic Metadata Analysis
         * ================================================================
         *
         * Each candidate method is combined with the Compilation so the
         * MetadataAnalyzer can obtain the appropriate SemanticModel.
         *
         * MetadataAnalyzer resolves:
         *   - IMethodSymbol
         *   - AutoLogAttribute
         *   - SensitiveAttribute
         *   - method identity
         *   - declaring type information
         *   - return type information
         *   - async/iterator/static state
         *   - CancellationToken usage
         *   - parameter metadata
         *   - policy/span values
         *
         * This class does not perform semantic analysis directly.
         */
        IncrementalValuesProvider<RawMethodMetadata?> analyzedMethods =
            methodDeclarations
                .Combine(context.CompilationProvider)
                .Select(static (pair, _) =>
                {
                    (MethodDeclarationSyntax methodDeclaration, Compilation compilation) = pair;

                    SemanticModel semanticModel =
                        compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

                    var analyzer = new MetadataAnalyzer(compilation);

                    return analyzer.Analyze(semanticModel, methodDeclaration);
                });

        /*
         * Remove methods that could not be analyzed successfully.
         *
         * MetadataAnalyzer returns null when:
         *   - the method symbol cannot be resolved
         *   - the AutoLog attribute is not present
         *   - required semantic information is unavailable
         */
        IncrementalValuesProvider<RawMethodMetadata> validAnalyzedMethods =
            analyzedMethods
                .Where(static metadata => metadata is not null)
                .Select(static (metadata, _) => metadata!);

        /*
         * ================================================================
         * Stage 3 — Metadata Building
         * ================================================================
         *
         * MetadataBuilder transforms RawMethodMetadata into finalized
         * generator-side metadata.
         *
         * MetadataBuilder performs no semantic analysis and generates
         * no source code. The resulting SentinelMethodGenerationMetadata
         * contains all information required by the vNext Instrumentation
         * Engine for method-body rewriting.
         */
        IncrementalValuesProvider<SentinelMethodGenerationMetadata> methodMetadataProvider =
            validAnalyzedMethods.Select(static (rawMetadata, _) =>
                MetadataBuilder.Build(rawMetadata));

        /*
         * ================================================================
         * Stage 4 — Instrumentation Engine Integration
         * ================================================================
         *
         * In vNext, the incremental generator does not emit source code.
         * Instead, the finalized metadata is consumed by the Instrumentation
         * Engine, which performs IL/method-body rewriting.
         *
         * No source emission occurs here.
         */
        // (No RegisterSourceOutput calls required in vNext.)
    }

    /// <summary>
    /// Performs syntax-only filtering for potential AutoLog methods.
    /// </summary>
    private static bool IsCandidateMethod(SyntaxNode node)
    {
        if (node is not MethodDeclarationSyntax method)
        {
            return false;
        }

        return method.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Any(static attribute => IsAutoLogAttributeSyntax(attribute));
    }

    /// <summary>
    /// Determines whether an attribute syntax node could represent
    /// the Sentinel Diagnostics AutoLog attribute.
    ///
    /// This method performs syntax-only filtering and does not resolve
    /// attribute symbols.
    /// </summary>
    private static bool IsAutoLogAttributeSyntax(AttributeSyntax attribute)
    {
        string name = attribute.Name.ToString();

        return name.Equals("AutoLog", StringComparison.Ordinal) ||
               name.Equals("AutoLogAttribute", StringComparison.Ordinal) ||
               name.EndsWith(".AutoLog", StringComparison.Ordinal) ||
               name.EndsWith(".AutoLogAttribute", StringComparison.Ordinal);
    }
}
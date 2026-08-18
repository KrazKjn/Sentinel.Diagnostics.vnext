using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Builders;
using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Models;
using Sentinel.Diagnostics.Generator.Validation;
using System;
using System.IO;
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

        var projectConfigProvider =
            context.AdditionalTextsProvider
                .Where(static file =>
                    Path.GetFileName(file.Path)
                        .Equals("sentinel.json", StringComparison.OrdinalIgnoreCase))
                .Select(static (file, _) =>
                {
                    var text = file.GetText()?.ToString();

                    return ProjectConfigurationLoader.Load(text);
                })
                .Collect()
                .Select(static (configs, _) =>
                    configs.Length > 0
                        ? configs[0]
                        : ProjectConfigurationLoader.Load(null))
                .WithComparer(ProjectAutoLogOptionsComparer.Instance);

        IncrementalValuesProvider<RawMethodMetadata?> analyzedMethods =
            methodDeclarations
                .Combine(context.CompilationProvider)
                .Combine(projectConfigProvider)
                .Select(static (triple, _) =>
                {
                    ((MethodDeclarationSyntax methodDeclaration, Compilation compilation), ProjectAutoLogOptions projectOptions) = triple;

                    SemanticModel semanticModel =
                        compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

                    var analyzer = new MetadataAnalyzer(compilation, projectOptions);

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
         * Stage 3 — Phase‑3 Validation + Stage 4 — Metadata Building
         * ================================================================
         *
         * Validation requires SourceProductionContext, so it must occur inside
         * RegisterSourceOutput.
         */

        // ---------------------------------------------------------------
        // Phase 4D — Combine validated methods with project config
        // ---------------------------------------------------------------
        var validatedMethodsWithConfig =
            validAnalyzedMethods.Combine(projectConfigProvider);

        context.RegisterSourceOutput(validatedMethodsWithConfig, (spc, pair) =>
        {
            /*
             * ================================================================
             * Stage 3 — Phase‑3 Validation
             * ================================================================
             *
             * MetadataValidator transforms RawMethodMetadata into ValidatedMethodMetadata.
             * Invalid methods produce diagnostics and are filtered out.
             *
             * This ensures:
             *   - AutoLog usage is valid
             *   - sensitive parameter rules are enforced
             *   - CancellationToken rules are enforced
             *   - span/policy values are valid
             *   - inherited attributes are resolved correctly
             *
             * Only validated metadata proceeds to Stage 4.
             */
            var (raw, projectOptions) = pair;

            var validated = MetadataValidator.Validate(raw, spc);
            if (validated is null)
                return;

            /*
             * ================================================================
             * Stage 4 — Metadata Building
             * ================================================================
             *
             * MetadataBuilder transforms ValidatedMethodMetadata into finalized
             * generator-side metadata (SentinelMethodGenerationMetadata).
             *
             * MetadataBuilder performs no semantic analysis and generates no source code.
             * The resulting metadata contains all information required by the vNext
             * Instrumentation Engine for method-body rewriting.
             */

            // Validate sentinel.json
            // Phase 4D — Validate sentinel.json
            SentinelConfigValidator.Validate(projectOptions.RawJson, spc);

            // Phase 4E — compute effective AutoLog options
            var effective = EffectiveOptionsBuilder.Build(validated, projectOptions);

            // Phase 5 will consume both validated metadata + effective options
            var built = MetadataBuilder.Build(validated, effective);

        });
        /*
         * ================================================================
         * Stage 5 — Instrumentation Engine Integration
         * ================================================================
         *
         * In vNext, the incremental generator does not emit source code.
         * Instead, the finalized metadata is consumed by the Instrumentation
         * Engine, which performs IL/method-body rewriting.
         *
         * No RegisterSourceOutput calls are required.
         */
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
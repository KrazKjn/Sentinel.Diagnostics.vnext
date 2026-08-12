using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Models;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;

namespace TestUtilities
{
    public class AnalysisTestHelpers
    {
        public static RawMethodMetadata? AnalyzeSingleMethod(string source)
        {
            Compilation compilation = TestCompilationFactory.CreateCompilation(source);
            var analyzer = new MetadataAnalyzer(compilation);

            var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());

            var root = tree.GetRoot();

            var methodDecl = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            var model = compilation.GetSemanticModel(tree);

            return analyzer.Analyze(model, methodDecl);
        }
    }
}

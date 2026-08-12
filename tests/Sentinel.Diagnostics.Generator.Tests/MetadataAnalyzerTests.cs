using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;
using System.Diagnostics;

namespace Sentinel.Diagnostics.Generator.Tests;

public sealed class MetadataAnalyzerTests
{
    [Fact]
    public void Analyze_ReturnsNull_WhenAutoLogMissing()
    {
        const string source = @"
            public class C
            {
                public void M(int x) { }
            }
        ";

        var compilation = TestCompilationFactory.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.Null(result);
    }

    [Fact]
    public void Analyze_ResolvesAutoLogAttribute()
    {
        //[AutoLog(""TestPolicy"", ""TestSpan"")]
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""TestPolicy"", Span = ""TestSpan"")]
                public void M(int x) { }
            }
        ";

        var compilation = TestCompilationFactory.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.NotNull(result);
        Assert.Equal("TestPolicy", result!.PolicyName);
        Assert.Equal("TestSpan", result.SpanName);
    }

    [Fact]
    public void Analyze_DetectsSensitiveParameter()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M([Sensitive] string password) { }
            }
        ";

        var compilation = TestCompilationFactory.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.NotNull(result);
        Assert.True(result!.Parameters.Single().IsSensitive);
    }

    [Fact]
    public void Analyze_DetectsDefaultParameterValue()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(int x = 42) { }
            }
        ";

        var compilation = TestCompilationFactory.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        var param = result!.Parameters.Single();

        Assert.True(param.HasExplicitDefaultValue);
        Assert.Equal("42", param.DefaultValueExpression);
    }

    [Fact]
    public void Analyze_DetectsIteratorMethod()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public System.Collections.Generic.IEnumerable<int> M()
                {
                    yield return 1;
                }
            }
        ";

        var compilation = TestCompilationFactory.CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.True(result!.IsIterator);
    }
}
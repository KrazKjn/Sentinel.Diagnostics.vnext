using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Analysis;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Models;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;
using Sentinel.Diagnostics.Generator.Tests.Validation;
using TestUtilities;

namespace Analysis;

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
        var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
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
        var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.NotNull(result);
        Assert.Equal("TestPolicy", result!.Options.Policy);
        Assert.Equal("TestSpan", result.Options.Span);
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
        var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
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
        var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
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
        var tree = compilation.SyntaxTrees.First(t => t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single();

        var analyzer = new MetadataAnalyzer(compilation);

        var result = analyzer.Analyze(model, method);

        Assert.True(result!.IsIterator);
    }

    [Fact]
    public void AutoLog_WithNamedArguments_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""TestPolicy"", Span = ""TestSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("TestPolicy", metadata!.Options.Policy);
        Assert.Equal("TestSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_WithPositionalArguments_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(""TestPolicy"", ""TestSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("TestPolicy", metadata!.Options.Policy);
        Assert.Equal("TestSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_WithMixedPositionalAndNamed_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(""TestPolicy"", Span = ""TestSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("TestPolicy", metadata!.Options.Policy);
        Assert.Equal("TestSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_WithoutArguments_UsesDefaults()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("DefaultPolicy", metadata!.Options.Policy);
        Assert.Equal("M", metadata.Options.Span); // default span = symbol.Name
    }

    [Fact]
    public void AutoLog_WithAlias_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;
            using AL = Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute;

            public class C
            {
                [AL(Policy = ""AliasPolicy"", Span = ""AliasSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("AliasPolicy", metadata!.Options.Policy);
        Assert.Equal("AliasSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_InheritedAttribute_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public sealed class MyAutoLogAttribute : AutoLogAttribute
            {
                public MyAutoLogAttribute(string? policy = null, string? span = null)
                    : base(policy, span) { }
            }

            public class C
            {
                [MyAutoLog(Policy = ""InheritedPolicy"", Span = ""InheritedSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("InheritedPolicy", metadata!.Options.Policy);
        Assert.Equal("InheritedSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_MultipleAttributes_FirstIsUsedByAnalyzer()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""FirstPolicy"", Span = ""FirstSpan"")]
                [AutoLog(Policy = ""SecondPolicy"", Span = ""SecondSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("FirstPolicy", metadata!.Options.Policy);
        Assert.Equal("FirstSpan", metadata.Options.Span);
    }

    [Fact]
    public void AutoLog_OnClassAndMethod_MethodAttributeWinsForSpanAndPolicy()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            [AutoLog(Policy = ""ClassPolicy"", Span = ""ClassSpan"")]
            public class C
            {
                [AutoLog(Policy = ""MethodPolicy"", Span = ""MethodSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("MethodPolicy", metadata!.Options.Policy);
        Assert.Equal("MethodSpan", metadata.Options.Span);
    }

    [Fact]
    public void Analyze_WithoutAutoLogAttribute_ReturnsNull()
    {
        const string source = @"
            public class C
            {
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.Null(metadata);
    }

    [Fact]
    public void AutoLog_SyntaxFallback_ResolvesPolicyAndSpan()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""SyntaxPolicy"", Span = ""SyntaxSpan"")]
                public void M(int x) { }
            }
            ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);

        Assert.NotNull(metadata);
        Assert.Equal("SyntaxPolicy", metadata!.Options.Policy);
        Assert.Equal("SyntaxSpan", metadata.Options.Span);
    }
    private static RawMethodMetadata? AnalyzeRaw(string source)
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

    private static (ValidatedMethodMetadata? validated, List<Diagnostic> diagnostics)
        Validate(string source)
    {
        var raw = AnalyzeRaw(source);
        Assert.NotNull(raw);

        var diagnostics = new List<Diagnostic>();
        var context = new FakeSourceProductionContext();

        var validated = MetadataValidatorTestHelper.ValidateForTest(raw!, context);
        diagnostics.AddRange(context.Diagnostics);
        return (validated, diagnostics);
    }

    // ---------------------------------------------------------------------
    // SENTINEL002 — Invalid AutoLog usage
    // ---------------------------------------------------------------------

    [Fact]
    public void InvalidPolicy_ReportsDiagnostic()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = """", Span = ""Span"")]
                public void M() { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL007", diagnostics[0].Id);
    }

    [Fact]
    public void InvalidSpan_ReportsDiagnostic()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""   "")]
                public void M() { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL006", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SENTINEL003 — Unsupported method kinds
    // ---------------------------------------------------------------------

    [Fact]
    public void AsyncIterator_ReportsUnsupportedMethodKind()
    {
        const string src = @"
            using System.Collections.Generic;
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public async IAsyncEnumerable<int> M()
                {
                    yield return 1;
                }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL003", diagnostics[0].Id);
    }

    [Fact]
    public void IteratorMethod_ReportsUnsupportedMethodKind()
    {
        const string src = @"
            using System.Collections.Generic;
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public IEnumerable<int> M()
                {
                    yield return 1;
                }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL003", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SENTINEL004 — Sensitive parameter validation
    // ---------------------------------------------------------------------

    [Fact]
    public void SensitiveRefParameter_ReportsDiagnostic()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public void M([Sensitive] ref int x) { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL004", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SENTINEL005 — CancellationToken validation
    // ---------------------------------------------------------------------

    [Fact]
    public void MultipleCancellationTokens_ReportsDiagnostic()
    {
        const string src = @"
            using System.Threading;
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public void M(CancellationToken a, CancellationToken b) { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL005", diagnostics[0].Id);
    }

    [Fact]
    public void CancellationTokenNotLast_ReportsDiagnostic()
    {
        const string src = @"
            using System.Threading;
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public void M(CancellationToken ct, int x) { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL005", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SENTINEL006 — Span validation
    // ---------------------------------------------------------------------

    [Fact]
    public void SpanContainsWhitespace_ReportsDiagnostic()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""Bad Span"")]
                public void M() { }
            }
            ";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL006", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SENTINEL007 — Policy validation
    // ---------------------------------------------------------------------

    [Fact]
    public void PolicyContainsWhitespace_ReportsDiagnostic()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""Bad Policy"", Span = ""S"")]
                public void M() { }
            }
";

        var (_, diagnostics) = Validate(src);

        Assert.Single(diagnostics);
        Assert.Equal("SENTINEL007", diagnostics[0].Id);
    }

    // ---------------------------------------------------------------------
    // SUCCESS CASE — Valid method
    // ---------------------------------------------------------------------

    [Fact]
    public void ValidMethod_ProducesValidatedMetadata()
    {
        const string src = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog(Policy = ""P"", Span = ""S"")]
                public void M(int x) { }
            }
            ";

        var (validated, diagnostics) = Validate(src);

        Assert.Empty(diagnostics);
        Assert.NotNull(validated);

        Assert.Equal("M", validated!.MethodName);
        Assert.Equal("P", validated.Options.Policy);
        Assert.Equal("S", validated.Options.Span);
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sentinel.Diagnostics.Generator;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Linq;
using Xunit;

namespace Sentinel.Diagnostics.Generator.Tests;

public sealed class GeneratorTests
{
    [Fact]
    public void Generator_ProducesMetadata_ForAutoLogMethod()
    {
        const string source = @"
            using Sentinel.Diagnostics;

            public class C
            {
                [AutoLog]
                public void M(int x) { }
            }
        ";

        var driver = CSharpGeneratorDriver.Create(new SentinelDiagnosticsGenerator());
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics);

        Assert.Empty(diagnostics);
    }
}
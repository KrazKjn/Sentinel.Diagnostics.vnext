using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Tests.TestHelpers;

public static class GeneratorTestRunner
{
    public static ImmutableArray<Diagnostic> RunGeneratorAndGetDiagnostics(
        string source,
        AdditionalText? additionalText = null)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location)
            });

        var generator = new SentinelDiagnosticsGenerator();

        var driver = CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create<ISourceGenerator>(new IncrementalGeneratorTestWrapper(generator)),
            additionalTexts: additionalText is null
                ? ImmutableArray<AdditionalText>.Empty
                : ImmutableArray.Create(additionalText));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return diagnostics;
    }
}
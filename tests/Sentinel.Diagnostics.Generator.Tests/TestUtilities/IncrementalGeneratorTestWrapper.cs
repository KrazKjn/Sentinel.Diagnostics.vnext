using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Tests.TestHelpers;

/*
public sealed class IncrementalGeneratorTestWrapper : ISourceGenerator
{
    private readonly IIncrementalGenerator _inner;

    public IncrementalGeneratorTestWrapper(IIncrementalGenerator inner)
    {
        _inner = inner;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // no-op — incremental generators do not use this
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Roslyn will call the incremental pipeline automatically.
        // We do nothing here.
    }
}
*/

public sealed class IncrementalGeneratorTestWrapper : ISourceGenerator
{
    private readonly IIncrementalGenerator _incremental;

    public IncrementalGeneratorTestWrapper(IIncrementalGenerator incremental)
    {
        _incremental = incremental;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // no-op
    }
    
    public void Execute(GeneratorExecutionContext context)
    {
        // Wrap incremental generator using Roslyn's public adapter
        //var adapter = new SourceGeneratorAdapter(_incremental);

        // Run the adapter as a classic source generator
        //adapter.Execute(context);
    }
    
    /*
    public void Execute2(GeneratorExecutionContext context)
    {
        // Build a driver that wraps the incremental generator
        GeneratorDriver driver = new CSharpGeneratorDriver(
            generators: ImmutableArray<ISourceGenerator>.Empty,
            additionalTexts: context.AdditionalFiles,
            optionsProvider: context.AnalyzerConfigOptions,
            driverOptions: default);

        // Attach incremental generator
        driver = driver.AddGenerators(ImmutableArray.Create(_incremental));

        // Run incremental pipeline
        driver = driver.RunGenerators(context.Compilation);

        // Extract results
        var result = driver.GetRunResult();

        // Forward diagnostics
        foreach (var diag in result.Diagnostics)
            context.ReportDiagnostic(diag);

        // Forward generated sources
        foreach (var gen in result.Results)
        {
            foreach (var src in gen.GeneratedSources)
                context.AddSource(src.HintName, src.SourceText);
        }
    }
    */
}
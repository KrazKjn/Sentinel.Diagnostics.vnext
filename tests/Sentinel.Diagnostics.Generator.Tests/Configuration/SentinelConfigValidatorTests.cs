using Microsoft.CodeAnalysis.Testing;
using Sentinel.Diagnostics.Generator.Tests.TestHelpers;
using Sentinel.Diagnostics.Generator;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Configuration;

public sealed class SentinelConfigValidatorTests
{
    // Minimal C# source that triggers the generator
    private string source = @"
        using Sentinel.Diagnostics;

        public class C
        {
            [AutoLog]
            public void M() { }
        }
    ";

    [Fact]
    public async Task InvalidJsonProducesDiagnostic()
    {
        var test = new CSharpSourceGeneratorTest<SentinelDiagnosticsGenerator, XUnitVerifier>
        {
            TestState =
            {
                Sources = { source },
                AdditionalFiles =
                {
                    ("sentinel.json", "{ invalid json }")
                },
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("SD0000")
                }
            }
        };

        await test.RunAsync();
    }

    [Fact]
    public void InvalidJsonProducesDiagnostic2()
    {
        var json = "{ invalid json }";

        var additional = SentinelJsonTestHelper.CreateSentinelJson(json);

        var diagnostics = GeneratorTestRunner.RunGeneratorAndGetDiagnostics(source, additional);

        Assert.Contains(diagnostics, d => d.Id == "SD0000");
    }

    [Fact]
    public void UnknownPropertyProducesDiagnostic()
    {
        var json = @"{ ""AutoLog"": { ""Foo"": true } }";

        var additional = SentinelJsonTestHelper.CreateSentinelJson(json);

        var diagnostics = GeneratorTestRunner.RunGeneratorAndGetDiagnostics(source, additional);

        Assert.Contains(diagnostics, d => d.Id == "SD0004");
    }

    [Fact]
    public void InvalidBooleanProducesDiagnostic()
    {
        var json = @"{ ""AutoLog"": { ""Enabled"": ""yes"" } }";

        var additional = SentinelJsonTestHelper.CreateSentinelJson(json);

        var diagnostics = GeneratorTestRunner.RunGeneratorAndGetDiagnostics(source, additional);

        Assert.Contains(diagnostics, d => d.Id == "SD0002");
    }
}

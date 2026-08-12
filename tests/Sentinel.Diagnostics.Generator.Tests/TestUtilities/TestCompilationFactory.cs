using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sentinel.Diagnostics.Generator.Tests.TestUtilities;

internal static class TestCompilationFactory
{
    private const string TestAttributesSource = @"
        namespace Sentinel.Diagnostics.Core.Attributes
        {
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public class AutoLogAttribute : System.Attribute
            {
                public AutoLogAttribute() { }

                public AutoLogAttribute(string? policy)
                {
                    Policy = policy;
                }

                public AutoLogAttribute(string? policy, string? span)
                {
                    Policy = policy;
                    Span = span;
                }

                public string? Policy { get; init; }
                public string? Span { get; init; }
            }

            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public sealed class SensitiveAttribute : System.Attribute
            {
            }
        }
        ";

    public static Compilation CreateCompilation(string source)
    {
        // Load the Core assembly from the src project output
        var coreAssemblyPath = Path.Combine(
            AppContext.BaseDirectory,
            "Sentinel.Diagnostics.Core.dll");

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(source),
                CSharpSyntaxTree.ParseText(TestAttributesSource)
            },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.CancellationToken).Assembly.Location),
                MetadataReference.CreateFromFile(coreAssemblyPath)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

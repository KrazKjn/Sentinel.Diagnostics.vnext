using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sentinel.Diagnostics.Generator.Tests.TestUtilities;

internal static class TestCompilationFactory
{
    public static Compilation CreateCompilation(string source)
    {
        // Load the Core assembly from the src project output
        var coreAssemblyPath = Path.Combine(
            AppContext.BaseDirectory,
            "Sentinel.Diagnostics.Core.dll");

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.CancellationToken).Assembly.Location),
                MetadataReference.CreateFromFile(coreAssemblyPath)
                //MetadataReference.CreateFromFile(typeof(Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(Sentinel.Diagnostics.Core.Attributes.SensitiveAttribute).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

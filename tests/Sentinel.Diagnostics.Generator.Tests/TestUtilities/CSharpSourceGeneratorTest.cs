using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

public class CSharpSourceGeneratorTest<TGenerator>
    : CSharpSourceGeneratorTest<TGenerator, XUnitVerifier>
    where TGenerator : ISourceGenerator, new()
{
}
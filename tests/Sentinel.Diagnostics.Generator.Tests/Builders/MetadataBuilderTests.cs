using Sentinel.Diagnostics.Generator.Builders;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Collections.Immutable;

namespace Builders;

public sealed class MetadataBuilderTests
{
    [Fact]
    public void Build_TransformsRawMetadataCorrectly()
    {
        var raw = new RawMethodMetadata(
            MethodName: "M",
            MethodLocation: null!,
            FullyQualifiedMethodName: "global::C.M",
            DeclaringNamespace: "C",
            DeclaringTypeName: "C",
            FullyQualifiedDeclaringTypeName: "global::C",
            ContainingTypes: ImmutableArray<ContainingTypeMetadata>.Empty,
            ReturnTypeName: "int",
            FullyQualifiedReturnTypeName: "global::System.Int32",
            RawSpan: "M",
            RawPolicy: null,
            IsAsync: false,
            IsIterator: false,
            IsStatic: false,
            IsGenericMethod: false,
            GenericTypeParameters: ImmutableArray<string>.Empty,
            HasCancellationToken: false,
            Parameters: ImmutableArray<RawParameterMetadata>.Empty,
            MethodAccessibility: Microsoft.CodeAnalysis.Accessibility.Public,
            DeclaringTypeAccessibility: Microsoft.CodeAnalysis.Accessibility.Public);

        var result = MetadataBuilder.Build(raw);

        Assert.Equal("M", result.MethodName);
        Assert.Equal("global::C.M", result.FullyQualifiedMethodName);
        Assert.Equal("int", result.ReturnTypeName);
        Assert.False(result.IsAsync);
    }
}
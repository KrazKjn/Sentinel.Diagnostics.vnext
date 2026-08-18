using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;

namespace Configuration;

public sealed class SpanResolverTests
{
    [Fact]
    public void AttributeSpan_Wins()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName: "TestMethod",
            Attribute: new AutoLogAttributeOptions
            {
                Span = "attr-span"
            }
        );

        var project = new ProjectAutoLogOptions
        {
            Span = "proj-span"
        };

        var result = SpanResolver.ResolveSpan(validated, project);

        Assert.Equal("attr-span", result);
    }

    [Fact]
    public void ProjectSpan_Used_When_No_Attribute()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName: "TestMethod",
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions
        {
            Span = "proj-span"
        };

        var result = SpanResolver.ResolveSpan(validated, project);

        Assert.Equal("proj-span", result);
    }

    [Fact]
    public void MethodName_Fallback_When_None_Provided()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName: "TestMethod",
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions();

        var result = SpanResolver.ResolveSpan(validated, project);

        Assert.Equal("TestMethod", result);
    }
}

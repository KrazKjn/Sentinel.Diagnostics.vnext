using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;

namespace Configuration;

public sealed class PolicyResolverTests
{
    [Fact]
    public void AttributePolicy_Wins()
    {
        var validated = ValidatedMethodFactory.Create(
            Attribute: new AutoLogAttributeOptions
            {
                Policy = "attr"
            }
        );

        var project = new ProjectAutoLogOptions
        {
            Policy = "proj"
        };

        var result = PolicyResolver.ResolvePolicy(validated, project);

        Assert.Equal("attr", result);
    }

    [Fact]
    public void ProjectPolicy_Used_When_No_Attribute()
    {
        var validated = ValidatedMethodFactory.Create(
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions
        {
            Policy = "proj"
        };

        var result = PolicyResolver.ResolvePolicy(validated, project);

        Assert.Equal("proj", result);
    }

    [Fact]
    public void DefaultPolicy_When_None_Provided()
    {
        var validated = ValidatedMethodFactory.Create(
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions();

        var result = PolicyResolver.ResolvePolicy(validated, project);

        Assert.Equal("default", result);
    }
}

//using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Configuration;

namespace Configuration;

public sealed class AutoLogOptionResolverTests
{
    [Fact]
    public void MethodOverridesTypeAndProject()
    {
        var method = new MethodAutoLogOptions { Enabled = false };
        var type = new TypeAutoLogOptions { Enabled = true };
        var project = new ProjectAutoLogOptions { Enabled = true };
        var defaults = new ProjectAutoLogOptions { Enabled = true };

        var result = AutoLogOptionResolver.Resolve(method, type, project, defaults);

        Assert.False(result.Enabled);
    }

    [Fact]
    public void TypeOverridesProject()
    {
        var method = new MethodAutoLogOptions();
        var type = new TypeAutoLogOptions { LogDuration = false };
        var project = new ProjectAutoLogOptions { LogDuration = true };
        var defaults = new ProjectAutoLogOptions { LogDuration = true };

        var result = AutoLogOptionResolver.Resolve(method, type, project, defaults);

        Assert.False(result.LogDuration);
    }

    [Fact]
    public void ProjectOverridesDefaults()
    {
        var method = new MethodAutoLogOptions();
        var type = new TypeAutoLogOptions();
        var project = new ProjectAutoLogOptions { Policy = "ProjectPolicy" };
        var defaults = new ProjectAutoLogOptions { Policy = "DefaultPolicy" };

        var result = AutoLogOptionResolver.Resolve(method, type, project, defaults);

        Assert.Equal("ProjectPolicy", result.Policy);
    }

    [Fact]
    public void DefaultsApplyWhenNothingSpecified()
    {
        var method = new MethodAutoLogOptions();
        var type = new TypeAutoLogOptions();
        var project = new ProjectAutoLogOptions();
        var defaults = new ProjectAutoLogOptions { Span = "DefaultSpan" };

        var result = AutoLogOptionResolver.Resolve(method, type, project, defaults);

        Assert.Equal("DefaultSpan", result.Span);
    }
}

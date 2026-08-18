using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Tests.TestUtilities;

namespace Configuration;

public sealed class EffectiveOptionsBuilderTests
{
    [Fact]
    public void AttributeOverrides_ProjectDefaults()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName:  "Test",
            Attribute: new AutoLogAttributeOptions
            {
                Enabled = false,
                AddUsing = false,
                AddTryCatch = true,
                LogParameters = false,
                LogDuration = true
            }
        );

        var project = new ProjectAutoLogOptions
        {
            Enabled = true,
            AddUsing = true,
            AddTryCatch = false,
            LogParameters = true,
            LogDuration = false
        };

        var effective = EffectiveOptionsBuilder.Build(validated, project);

        Assert.False(effective.Enabled);
        Assert.False(effective.AddUsing);
        Assert.True(effective.AddTryCatch);
        Assert.False(effective.LogParameters);
        Assert.True(effective.LogDuration);
    }

    [Fact]
    public void ProjectDefaults_Used_When_No_Attribute_Overrides()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName: "Test",
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions
        {
            Enabled = false,
            AddUsing = false,
            AddTryCatch = true,
            LogParameters = false,
            LogDuration = true
        };

        var effective = EffectiveOptionsBuilder.Build(validated, project);

        Assert.False(effective.Enabled);
        Assert.False(effective.AddUsing);
        Assert.True(effective.AddTryCatch);
        Assert.False(effective.LogParameters);
        Assert.True(effective.LogDuration);
    }

    [Fact]
    public void Fallbacks_Applied_When_No_Attribute_Or_Project()
    {
        var validated = ValidatedMethodFactory.Create(
            MethodName: "Test",
            Attribute: new AutoLogAttributeOptions()
        );

        var project = new ProjectAutoLogOptions();

        var effective = EffectiveOptionsBuilder.Build(validated, project);

        Assert.True(effective.Enabled);
        Assert.True(effective.AddUsing);
        Assert.False(effective.AddTryCatch);
        Assert.True(effective.LogParameters);
        Assert.True(effective.LogDuration);
    }
}

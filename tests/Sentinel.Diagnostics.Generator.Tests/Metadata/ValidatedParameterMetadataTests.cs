using Microsoft.CodeAnalysis;
using TestUtilities;
//using Sentinel.Diagnostics.Generator.Tests.TestUtilities;

namespace Metadata;

public sealed class ValidatedParameterMetadataTests
{
    // ---------------------------------------------------------------
    // Type Resolution
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_Type_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(int x, System.String y) { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        Assert.Equal("x", p[0].Name);
        Assert.Equal("global::System.Int32", p[0].FullyQualifiedTypeName);

        Assert.Equal("y", p[1].Name);
        Assert.Equal("global::System.String", p[1].FullyQualifiedTypeName);
    }

    // ---------------------------------------------------------------
    // RefKind
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_RefKind_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(ref int x, out string y, in double z)
                {
                    y = ""test"";
                }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        Assert.Equal(RefKind.Ref, p[0].RefKind);
        Assert.Equal(RefKind.Out, p[1].RefKind);
        Assert.Equal(RefKind.In, p[2].RefKind);
    }

    // ---------------------------------------------------------------
    // Params
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_Params_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(params string[] values) { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters[0];

        Assert.True(p.IsParams);
        Assert.Equal("global::System.String[]", p.FullyQualifiedTypeName);
    }

    // ---------------------------------------------------------------
    // Nullability
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_Nullability_Is_Validated_Correctly()
    {
        const string source = @"
            #nullable enable
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(string? name, string value) { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        Assert.True(p[0].IsNullable);
        Assert.False(p[1].IsNullable);
    }

    // ---------------------------------------------------------------
    // Default Values
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_DefaultValue_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(int x = 5, string s = ""abc"") { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        Assert.True(p[0].HasExplicitDefaultValue);
        Assert.Equal("5", p[0].DefaultValueExpression);

        Assert.True(p[1].HasExplicitDefaultValue);
        Assert.Equal("\"abc\"", p[1].DefaultValueExpression);
    }

    // ---------------------------------------------------------------
    // SensitiveAttribute
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_SensitiveAttribute_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M([Sensitive] string password) { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters[0];

        Assert.True(p.IsSensitive);
        Assert.False(p.ShouldLog);
    }

    // ---------------------------------------------------------------
    // ShouldLog Rules
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_ShouldLog_Is_Validated_Correctly()
    {
        const string source = @"
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(string normal, [Sensitive] string secret) { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        Assert.True(p[0].ShouldLog);
        Assert.False(p[1].ShouldLog);
    }

    // ---------------------------------------------------------------
    // Combined Cases
    // ---------------------------------------------------------------
    [Fact]
    public void Parameter_Combined_Cases_Are_Validated_Correctly()
    {
        const string source = @"
            #nullable enable
            using Sentinel.Diagnostics.Core.Attributes;

            public class C
            {
                [AutoLog]
                public void M(
                    ref string? name,
                    params int[] values,
                    [Sensitive] string? token = null)
                { }
            }
        ";

        var metadata = AnalysisTestHelpers.AnalyzeSingleMethod(source);
        var p = metadata!.Parameters;

        // ref string?
        Assert.Equal(RefKind.Ref, p[0].RefKind);
        Assert.True(p[0].IsNullable);

        // params int[]
        Assert.True(p[1].IsParams);

        // sensitive + nullable + default
        Assert.True(p[2].IsSensitive);
        Assert.False(p[2].ShouldLog);
        Assert.True(p[2].IsNullable);
        Assert.True(p[2].HasExplicitDefaultValue);
        Assert.Equal("null", p[2].DefaultValueExpression);
    }
}
using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Collections.Immutable;

namespace Sentinel.Diagnostics.Generator.Tests.TestUtilities;

public static class ValidatedMethodFactory
{
    public static ValidatedMethodMetadata Create(
        string MethodName = "TestMethod",
        AutoLogAttributeOptions? Attribute = null,
        EffectiveAutoLogOptions? Options = null)
    {
        return new ValidatedMethodMetadata(
            MethodName: MethodName,
            MethodLocation: Location.None,
            ReturnTypeName: "void",
            FullyQualifiedReturnTypeName: "void",
            FullyQualifiedMethodName: "TestNamespace.TestClass.TestMethod",
            DeclaringNamespace: "TestNamespace",
            DeclaringTypeName: "TestClass",
            FullyQualifiedDeclaringTypeName: "TestNamespace.TestClass",
            ContainingTypes: ImmutableArray<ContainingTypeMetadata>.Empty,
            Options: Options ?? new EffectiveAutoLogOptions
            {
                Span = "M",
                Policy = "Default",
                Enabled = true,
                AddUsing = true,
                AddTryCatch = true,
                LogParameters = true,
                LogDuration = true
            },
            Attribute: Attribute ?? new AutoLogAttributeOptions
            {
                Span = "M",
                Policy = "Default",
                Enabled = true,
                AddUsing = true,
                AddTryCatch = true,
                LogParameters = true,
                LogDuration = true
            },
            IsAsync: false,
            IsStatic: false,
            IsIterator: false,
            HasCancellationToken: false,
            HasSensitiveParameters: false,
            GenericTypeParameters: ImmutableArray<string>.Empty,
            SensitiveParameterNames: ImmutableArray<string>.Empty,
            MethodAccessibility: Accessibility.Public,
            DeclaringTypeAccessibility: Accessibility.Public,
            Parameters: ImmutableArray<ValidatedParameterMetadata>.Empty
        );
    }
}
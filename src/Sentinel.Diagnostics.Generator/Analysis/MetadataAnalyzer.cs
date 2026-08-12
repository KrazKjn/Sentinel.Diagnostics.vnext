using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Configuration;
using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sentinel.Diagnostics.Generator.Analysis;

/// <summary>
/// Performs semantic analysis on methods identified by the syntax discovery
/// stage of the Sentinel Diagnostics incremental source generator.
/// </summary>
public sealed class MetadataAnalyzer(Compilation compilation)
{
    private readonly INamedTypeSymbol? _autoLogSymbol =
            compilation.GetTypeByMetadataName("Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute");
    private readonly INamedTypeSymbol? _sensitiveSymbol =
            compilation.GetTypeByMetadataName("Sentinel.Diagnostics.Core.Attributes.SensitiveAttribute");
    private readonly INamedTypeSymbol? _cancellationTokenSymbol =
            compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
    private static readonly SymbolDisplayFormat FullyQualifiedTypeFormat =
        new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeParameters |
                SymbolDisplayGenericsOptions.IncludeVariance,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes,
            memberOptions: SymbolDisplayMemberOptions.None
        );

    /// <summary>
    /// Analyzes a method declaration using the supplied SemanticModel.
    /// </summary>
    public RawMethodMetadata? Analyze(
        SemanticModel semanticModel,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (_autoLogSymbol is null)
        {
            // Attribute not found in compilation — nothing to analyze.
            return null;
        }

        if (semanticModel.GetDeclaredSymbol(methodDeclaration)
            is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        //AutoLogAttributeData? autoLog =
        //    GetAutoLogAttribute(methodSymbol, methodDeclaration);

        var autoLogAttributes = GetAutoLogAttributes(methodSymbol, methodDeclaration);

        if (autoLogAttributes.Count == 0)
            return null;

        // TODO: Handle multiple AutoLog attributes on the same method (e.g., for different policies).
        AutoLogAttributeData autoLog = autoLogAttributes[0];

        if (autoLog is null)
        {
            return null;
        }

        ImmutableArray<ContainingTypeMetadata> containingTypes =
            BuildContainingTypeMetadata(methodSymbol.ContainingType);

        ImmutableArray<RawParameterMetadata> parameters =
            BuildParameterMetadata(methodSymbol);

        string fullyQualifiedMethodName =
            methodSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat);

        string returnTypeName =
            methodSymbol.ReturnType.ToDisplayString(
                SymbolDisplayFormat.MinimallyQualifiedFormat);

        string? fullyQualifiedReturnTypeName =
            methodSymbol.ReturnType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat);

        bool isAsync = methodSymbol.IsAsync;

        bool isIterator = IsIteratorMethod(methodDeclaration);

        bool isStatic = methodSymbol.IsStatic;

        bool hasCancellationToken =
            methodSymbol.Parameters.Any(parameter =>
                SymbolEqualityComparer.Default.Equals(
                    parameter.Type,
                    _cancellationTokenSymbol));

        // Namespace containing the declaring type.
        string declaringNamespace =
            methodSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : methodSymbol.ContainingNamespace.ToDisplayString();

        // Name of the immediate declaring type.
        string declaringTypeName =
            methodSymbol.ContainingType.Name;

        // Fully qualified declaring type, including global::.
        string fullyQualifiedDeclaringTypeName =
            methodSymbol.ContainingType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat);

        string spanName =
            autoLog.Span ?? methodSymbol.Name;

        // TODO: Resolve options from project, containing type, and method attributes.
        //EffectiveAutoLogOptions options =
        //    ConfigurationResolver.Resolve(
        //        projectOptions,
        //        containingTypeOptions,
        //        methodOptions,
        //        methodSymbol.Name);

        EffectiveAutoLogOptions options =
            new EffectiveAutoLogOptions
            {
                Enabled = true,
                AddUsing = true,
                AddTryCatch = true,
                LogParameters = true,
                LogDuration = true,
                Policy = autoLog.Policy ?? "Default",
                Span = autoLog.Span ?? methodSymbol.Name
            };

        return new RawMethodMetadata(
            MethodName: methodSymbol.Name,
            MethodLocation: methodDeclaration.GetLocation(),
            FullyQualifiedMethodName: fullyQualifiedMethodName,

            DeclaringNamespace: declaringNamespace,
            DeclaringTypeName: declaringTypeName,
            FullyQualifiedDeclaringTypeName:
                fullyQualifiedDeclaringTypeName,
            ContainingTypes: containingTypes,

            ReturnTypeName: returnTypeName,
            FullyQualifiedReturnTypeName: fullyQualifiedReturnTypeName,

            IsAsync: isAsync,
            IsIterator: isIterator,
            IsStatic: isStatic,
            HasCancellationToken: hasCancellationToken,
            IsGenericMethod: methodSymbol.IsGenericMethod,
            GenericTypeParameters: methodSymbol.TypeParameters
                .Select(static parameter => parameter.Name)
                .ToImmutableArray(),

            Parameters: parameters,
            Options: options,

            MethodAccessibility: methodSymbol.DeclaredAccessibility,
            DeclaringTypeAccessibility: methodSymbol.ContainingType.DeclaredAccessibility);
    }

    /// <summary>
    /// Extracts parameter metadata from the resolved method symbol.
    /// </summary>

    private ImmutableArray<RawParameterMetadata> BuildParameterMetadata(
        IMethodSymbol methodSymbol)
    {
        var builder =
            ImmutableArray.CreateBuilder<RawParameterMetadata>(
                methodSymbol.Parameters.Length);

        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            string parameterType =
                parameter.Type.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat);

            //string? fullyQualifiedParameterType = parameter.Type.ToDisplayString(FullyQualifiedTypeFormat);
            string? fullyQualifiedParameterType = GetFullyQualifiedType(parameter.Type);

            bool isSensitive = IsSensitiveParameter(parameter, _sensitiveSymbol);

            bool shouldLog = isSensitive ? false : true; // TODO: support [NoLog], default to Not showing Sensitive data.
                                                         // Should support a [NoLog] attribute or a property on the AutoLog attribute to indicate that this parameter should not be logged.
                                                         // Should allow for Showing Sensitive data if explicitly requested, but default to not showing it.

            string? defaultValueExpression =
                parameter.HasExplicitDefaultValue
                    ? GetDefaultValueExpression(parameter)
                    : null;

            builder.Add(
                new RawParameterMetadata(
                    Name: parameter.Name,
                    TypeName: parameterType,
                    FullyQualifiedTypeName: fullyQualifiedParameterType,
                    RefKind: parameter.RefKind,
                    IsParams: parameter.IsParams,
                    IsNullable: parameter.NullableAnnotation == NullableAnnotation.Annotated,
                    IsSensitive: isSensitive,
                    ShouldLog: shouldLog,
                    HasExplicitDefaultValue: parameter.HasExplicitDefaultValue,
                    DefaultValueExpression: defaultValueExpression));
        }

        return builder.ToImmutable();
    }

    private static string? GetDefaultValueExpression(
        IParameterSymbol parameter)
    {
        if (!parameter.HasExplicitDefaultValue)
        {
            return null;
        }

        object? value = parameter.ExplicitDefaultValue;

        if (value is null)
        {
            return "null";
        }

        return value switch
        {
            string stringValue =>
                SymbolDisplay.FormatLiteral(
                    stringValue,
                    quote: true),

            char charValue =>
                SymbolDisplay.FormatLiteral(
                    charValue,
                    quote: true),

            bool boolValue =>
                boolValue ? "true" : "false",

            double doubleValue =>
                doubleValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            float floatValue =>
                floatValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + "F",

            decimal decimalValue =>
                decimalValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + "M",

            long longValue =>
                longValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + "L",

            ulong ulongValue =>
                ulongValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + "UL",

            int intValue =>
                intValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            uint uintValue =>
                uintValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) + "U",

            short shortValue =>
                shortValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            ushort ushortValue =>
                ushortValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            byte byteValue =>
                byteValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            sbyte sbyteValue =>
                sbyteValue.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),

            _ => null
        };
    }

    /// <summary>
    /// Resolves the AutoLog attribute using symbol comparison.
    /// Supports both named and positional constructor arguments.
    /// </summary>
    /*
    private AutoLogAttributeData? GetAutoLogAttribute(
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclaration)
    {
        foreach (var attributeData in methodSymbol.GetAttributes())
        {
            if (!IsAutoLogAttribute(attributeData))
                continue;

            string? policy = null;
            string? span = null;

            // --- Semantic binding first ---
            // Positional constructor args
            if (attributeData.ConstructorArguments.Length > 0)
            {
                if (attributeData.ConstructorArguments[0].Value is string s)
                    policy = s;

                if (attributeData.ConstructorArguments.Length > 1 &&
                    attributeData.ConstructorArguments[1].Value is string s2)
                    span = s2;
            }

            // Named args
            foreach (var named in attributeData.NamedArguments)
            {
                if (named.Key == "Policy")
                    policy = named.Value.Value as string;

                if (named.Key == "Span")
                    span = named.Value.Value as string;
            }

            // --- Syntax fallback ---
            if (policy is null || span is null)
            {
                foreach (var attrList in methodDeclaration.AttributeLists)
                {
                    foreach (var attrSyntax in attrList.Attributes)
                    {
                        // Match attribute name or alias
                        var name = attrSyntax.Name.ToString();
                        if (!name.EndsWith("AutoLog") &&
                            !name.EndsWith("AutoLogAttribute"))
                            continue;

                        var args = ExtractAttributeArguments(attrSyntax);

                        if (policy is null)
                        {
                            if (args.TryGetValue("Policy", out var p))
                                policy = p;
                            else if (args.TryGetValue("_arg0", out var p0))
                                policy = p0;
                        }

                        if (span is null)
                        {
                            if (args.TryGetValue("Span", out var s))
                                span = s;
                            else if (args.TryGetValue("_arg1", out var s1))
                                span = s1;
                        }
                    }
                }
            }

            return new AutoLogAttributeData(policy, span);
        }

        return null;
    }
    */

    /// <summary>
    /// Extracts all AutoLog attributes applied to any symbol (method, class, interface, property).
    /// Supports semantic binding, syntax fallback, inheritance, aliases, positional + named args.
    /// </summary>
    private static IReadOnlyList<AutoLogAttributeData> GetAutoLogAttributes(
        ISymbol symbol,
        SyntaxNode declarationSyntax)
    {
        var results = new List<AutoLogAttributeData>();

        foreach (var attributeData in symbol.GetAttributes())
        {
            if (!IsAutoLogAttribute(attributeData))
                continue;

            string? policy = null;
            string? span = null;

            // -------------------------------
            // 1. SEMANTIC BINDING (preferred)
            // -------------------------------

            // Positional constructor arguments
            if (attributeData.ConstructorArguments.Length > 0)
            {
                if (attributeData.ConstructorArguments[0].Value is string p)
                    policy = p;

                if (attributeData.ConstructorArguments.Length > 1 &&
                    attributeData.ConstructorArguments[1].Value is string s)
                    span = s;
            }

            // Named arguments
            foreach (var named in attributeData.NamedArguments)
            {
                if (named.Key == "Policy")
                    policy = named.Value.Value as string;

                if (named.Key == "Span")
                    span = named.Value.Value as string;
            }

            // -------------------------------
            // 2. SYNTAX FALLBACK (always works)
            // -------------------------------

            if (policy is null || span is null)
            {
                foreach (var attrList in declarationSyntax.DescendantNodesAndSelf().OfType<AttributeListSyntax>())
                {
                    foreach (var attrSyntax in attrList.Attributes)
                    {
                        if (!AttributeNameMatches(attrSyntax.Name))
                            continue;

                        var args = ExtractAttributeArguments(attrSyntax);

                        // Named
                        if (policy is null && args.TryGetValue("Policy", out var p))
                            policy = p;

                        if (span is null && args.TryGetValue("Span", out var s))
                            span = s;

                        // Positional
                        if (policy is null && args.TryGetValue("_arg0", out var p0))
                            policy = p0;

                        if (span is null && args.TryGetValue("_arg1", out var s1))
                            span = s1;
                    }
                }
            }

            // -------------------------------
            // 3. DEFAULT VALUES (optional)
            // -------------------------------

            policy ??= "DefaultPolicy";
            span ??= symbol.Name; // default span = method/class/property name

            //results.Add(new AutoLogAttributeData(policy, span, true));
            results.Add(new AutoLogAttributeData(
                Policy: policy,
                Span: span,
                Enabled: true,
                AddUsing: true,
                AddTryCatch: true,
                LogParameters: true,
                LogDuration: true));
        }

        return results;
    }


    private static bool IsAutoLogAttribute(AttributeData attribute)
    {
        var attrClass = attribute.AttributeClass;
        if (attrClass == null)
            return false;
        //return attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        //    == "global::Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute";
        return attrClass.Name == "AutoLogAttribute"
            || attrClass.BaseType?.Name == "AutoLogAttribute";
    }

    private static bool IsSensitiveAttribute(AttributeData attribute)
    {
        var attrClass = attribute.AttributeClass;
        if (attrClass == null)
            return false;
        //return attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        //    == "global::Sentinel.Diagnostics.Core.Attributes.SensitiveAttribute";
        return attrClass.Name == "SensitiveAttribute"
            || attrClass.BaseType?.Name == "SensitiveAttribute";
    }

    private static bool IsSensitiveParameter(IParameterSymbol parameterSymbol, INamedTypeSymbol? sensitiveSymbol)
    {
        if (sensitiveSymbol is null)
        {
            return false;
        }
        return parameterSymbol.GetAttributes().Any(attribute =>
                IsSensitiveAttribute(attribute));
    }

    private static string? GetStringValue(TypedConstant value)
    {
        return value.Kind == TypedConstantKind.Primitive &&
               value.Value is string s
            ? s
            : null;
    }

    private static bool IsIteratorMethod(MethodDeclarationSyntax method)
    {
        return method.Body?.DescendantNodes().Any(
            n => n is YieldStatementSyntax) ?? false;
    }

    private static ImmutableArray<ContainingTypeMetadata> BuildContainingTypeMetadata(
        INamedTypeSymbol declaringType)
    {
        var types = ImmutableArray.CreateBuilder<ContainingTypeMetadata>();

        INamedTypeSymbol? current = declaringType;

        while (current is not null)
        {
            types.Insert(0, CreateContainingTypeMetadata(current));
            current = current.ContainingType;
        }

        return types.ToImmutable();
    }

    private static ContainingTypeMetadata CreateContainingTypeMetadata(
        INamedTypeSymbol typeSymbol)
    {
        ImmutableArray<string> typeParameters =
            typeSymbol.TypeParameters
                .Select(static parameter => parameter.Name)
                .ToImmutableArray();

        return new ContainingTypeMetadata(
            Name: typeSymbol.Name,
            FullyQualifiedName: typeSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat),
            Namespace: typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Accessibility: typeSymbol.DeclaredAccessibility,
            TypeKind: typeSymbol.TypeKind,
            TypeParameters: typeParameters);
    }

    private static Dictionary<string, string?> ExtractAttributeArguments_old(AttributeSyntax attributeSyntax)
    {
        var results = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (attributeSyntax.ArgumentList is null)
            return results;

        int positionalIndex = 0;

        foreach (var arg in attributeSyntax.ArgumentList.Arguments)
        {
            string name;
            string? value;

            // Named argument: Policy = "TestPolicy"
            if (arg.NameEquals is not null)
            {
                name = arg.NameEquals.Name.Identifier.Text;
            }
            // Named argument with colon syntax: Policy: "TestPolicy"
            else if (arg.NameColon is not null)
            {
                name = arg.NameColon.Name.Identifier.Text;
            }
            // Positional argument: "TestPolicy"
            else
            {
                name = $"_arg{positionalIndex++}";
            }

            // Extract literal or expression text
            value = arg.Expression switch
            {
                LiteralExpressionSyntax literal => literal.Token.ValueText,
                IdentifierNameSyntax ident => ident.Identifier.Text,
                MemberAccessExpressionSyntax member => member.ToString(),
                InvocationExpressionSyntax invocation => invocation.ToString(),
                _ => arg.Expression.ToString().Trim('"')
            };

            results[name] = value;
        }

        return results;
    }
    private static bool AttributeNameMatches(NameSyntax name)
    {
        // Handles: AutoLog, AutoLogAttribute, AL, Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute
        var identifier = name switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            QualifiedNameSyntax q => q.Right.Identifier.Text,
            AliasQualifiedNameSyntax a => a.Alias.Identifier.Text,
            _ => name.ToString()
        };

        return identifier == "AutoLog"
            || identifier == "AutoLogAttribute"
            || identifier == "Sentinel.Diagnostics.Core.Attributes.AutoLogAttribute"
            || identifier == "AL"; // alias support
    }

    private static Dictionary<string, string?> ExtractAttributeArguments(AttributeSyntax attributeSyntax)
    {
        var results = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (attributeSyntax.ArgumentList is null)
            return results;

        int positionalIndex = 0;

        foreach (var arg in attributeSyntax.ArgumentList.Arguments)
        {
            string name;

            if (arg.NameEquals is not null)
                name = arg.NameEquals.Name.Identifier.Text;
            else if (arg.NameColon is not null)
                name = arg.NameColon.Name.Identifier.Text;
            else
                name = $"_arg{positionalIndex++}";

            string? value = arg.Expression switch
            {
                LiteralExpressionSyntax literal => literal.Token.ValueText,
                IdentifierNameSyntax ident => ident.Identifier.Text,
                MemberAccessExpressionSyntax member => member.ToString(),
                InvocationExpressionSyntax invocation => invocation.ToString(),
                _ => arg.Expression.ToString().Trim('"')
            };

            results[name] = value;
        }

        return results;
    }

    private static string GetFullyQualifiedType(ITypeSymbol type)
    {
        // Handle arrays
        if (type is IArrayTypeSymbol array)
        {
            var element = array.ElementType;
            var elementName = GetFullyQualifiedType(element); // recursive!
            return $"{elementName}{GetArraySuffix(array)}";
        }

        // Handle special types manually
        if (type.SpecialType != SpecialType.None)
        {
            return $"global::{type.ContainingNamespace}.{type.Name}";
        }

        // Normal types
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string GetArraySuffix(IArrayTypeSymbol array)
    {
        if (array.Rank == 1)
            return "[]";

        return "[" + new string(',', array.Rank - 1) + "]";
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Generator.Metadata;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sentinel.Diagnostics.Generator.Analysis;

/// <summary>
/// Performs semantic analysis on methods identified by the syntax discovery
/// stage of the Sentinel Diagnostics incremental source generator.
/// </summary>
internal sealed class MetadataAnalyzer(Compilation compilation)
{
    private readonly INamedTypeSymbol? _autoLogSymbol =
            compilation.GetTypeByMetadataName("Sentinel.Diagnostics.AutoLogAttribute");
    private readonly INamedTypeSymbol? _sensitiveSymbol =
            compilation.GetTypeByMetadataName("Sentinel.Diagnostics.SensitiveAttribute");
    private readonly INamedTypeSymbol? _cancellationTokenSymbol =
            compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

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

        AutoLogAttributeData? autoLog =
            GetAutoLogAttribute(methodSymbol);

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
            SpanName: spanName,
            PolicyName: autoLog.Policy,

            IsAsync: isAsync,
            IsIterator: isIterator,
            IsStatic: isStatic,
            HasCancellationToken: hasCancellationToken,
            IsGenericMethod: methodSymbol.IsGenericMethod,
            GenericTypeParameters: methodSymbol.TypeParameters
                .Select(static parameter => parameter.Name)
                .ToImmutableArray(),

            Parameters: parameters,

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

            string? fullyQualifiedParameterType =
                parameter.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            bool isSensitive =
                _sensitiveSymbol is not null &&
                parameter.GetAttributes().Any(attribute =>
                    SymbolEqualityComparer.Default.Equals(
                        attribute.AttributeClass,
                        _sensitiveSymbol));

            bool shouldLog = true; // Future: support [NoLog]

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

    /*
    private ImmutableArray<RawParameterMetadata> BuildParameterMetadata(
        IMethodSymbol methodSymbol)
    {
        var builder = ImmutableArray.CreateBuilder<RawParameterMetadata>(
            methodSymbol.Parameters.Length);

        foreach (IParameterSymbol parameter in methodSymbol.Parameters)
        {
            string parameterType =
                parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            bool isSensitive =
                _sensitiveSymbol is not null &&
                parameter.GetAttributes().Any(attr =>
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, _sensitiveSymbol));

            bool shouldLog = true; // Future: support [NoLog]

            builder.Add(new RawParameterMetadata(
                Name: parameter.Name,
                ParameterType: parameterType,
                IsSensitive: isSensitive,
                ShouldLog: shouldLog));
        }

        return builder.ToImmutable();
    }*/

    /// <summary>
    /// Resolves the AutoLog attribute using symbol comparison.
    /// Supports both named and positional constructor arguments.
    /// </summary>
    private AutoLogAttributeData? GetAutoLogAttribute(IMethodSymbol methodSymbol)
    {
        foreach (AttributeData attribute in methodSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _autoLogSymbol))
            {
                continue;
            }

            string? policy = null;
            string? span = null;

            // Positional constructor arguments
            if (attribute.ConstructorArguments.Length > 0)
            {
                TypedConstant firstArg = attribute.ConstructorArguments[0];
                if (firstArg.Kind == TypedConstantKind.Primitive &&
                    firstArg.Value is string s)
                {
                    policy = s;
                }
            }

            // Named arguments
            foreach (KeyValuePair<string, TypedConstant> named in attribute.NamedArguments)
            {
                switch (named.Key)
                {
                    case "Policy":
                        policy = GetStringValue(named.Value);
                        break;

                    case "Span":
                        span = GetStringValue(named.Value);
                        break;
                }
            }

            return new AutoLogAttributeData(policy, span);
        }

        return null;
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
}

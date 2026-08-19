namespace Sentinel.Diagnostics.Core.Metadata
{
    public enum SentinelMethodType
    {
        /// <summary>
        /// A normal method (instance, static, async, iterator, generic).
        /// Roslyn: MethodDeclarationSyntax
        /// Reflection: MethodInfo (non-special-name)
        /// </summary>
        Method,

        /// <summary>
        /// A constructor (.ctor or .cctor).
        /// Roslyn: ConstructorDeclarationSyntax
        /// Reflection: ConstructorInfo
        /// </summary>
        Constructor,

        /// <summary>
        /// A property getter (get_X).
        /// Roslyn: AccessorDeclarationSyntax (GetAccessorDeclaration)
        /// Reflection: MethodInfo with IsSpecialName && Name.StartsWith("get_")
        /// </summary>
        PropertyGetter,

        /// <summary>
        /// A property setter (set_X).
        /// Roslyn: AccessorDeclarationSyntax (SetAccessorDeclaration)
        /// Reflection: MethodInfo with IsSpecialName && Name.StartsWith("set_")
        /// </summary>
        PropertySetter,

        /// <summary>
        /// A user-defined operator (op_Addition, op_Equality, implicit/explicit).
        /// Roslyn: OperatorDeclarationSyntax, ConversionOperatorDeclarationSyntax
        /// Reflection: MethodInfo with IsSpecialName && Name.StartsWith("op_")
        /// </summary>
        Operator,

        /// <summary>
        /// A local function declared inside a method.
        /// Roslyn: LocalFunctionStatementSyntax
        /// Reflection: MethodInfo with metadata name containing "<>g__"
        /// </summary>
        LocalFunction,

        /// <summary>
        /// A lambda expression or anonymous method.
        /// Roslyn: LambdaExpressionSyntax, AnonymousMethodExpressionSyntax
        /// Reflection: MethodInfo with metadata name containing "b__"
        /// </summary>
        Lambda,

        /// <summary>
        /// A delegate invocation method (Invoke, BeginInvoke, EndInvoke).
        /// Roslyn: Not directly represented; detected via semantic model.
        /// Reflection: MethodInfo on delegate types
        /// </summary>
        DelegateInvoke
    }
}

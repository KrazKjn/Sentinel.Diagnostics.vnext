using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Sentinel.Diagnostics.Cli.Rewriting;

public sealed class AutoLogSyntaxRewriter : CSharpSyntaxRewriter
{
    private string _currentNamespace = string.Empty;
    private string _currentClass = string.Empty;

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _currentNamespace = node.Name.ToString();
        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        _currentNamespace = node.Name.ToString();
        return base.VisitFileScopedNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _currentClass = node.Identifier.Text;
        return base.VisitClassDeclaration(node);
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Only rewrite methods marked with [AutoLog]
        if (!HasAutoLogAttribute(node))
            return base.VisitMethodDeclaration(node);

        // Only rewrite methods with block bodies.
        if (node.Body == null)
            return base.VisitMethodDeclaration(node);

        // Build the generated body.
        var block = BuildAutoLoggerWrappedBody(node);

        // Format ONLY the block we created
        var formattedBlock = (BlockSyntax)block.NormalizeWhitespace();

        // Insert formatted block into method, leave user code alone
        return node.WithBody(formattedBlock)
            // Add a trailing newline after the adding the new function block for formatting
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        // Build the generated body.
        //
        // IMPORTANT:
        // Do NOT NormalizeWhitespace() here.
        //
        // The body carries Formatter.Annotation and will be
        // formatted after it has been inserted into the original
        // syntax tree.
        //var generatedBody =
        //    BuildAutoLoggerWrappedBody(node);

        //return node.WithBody(generatedBody);
    }

    private static bool HasAutoLogAttribute(MethodDeclarationSyntax node)
    {
        return node.AttributeLists
            .SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString() == "AutoLog");
    }

    private BlockSyntax BuildAutoLoggerWrappedBody(MethodDeclarationSyntax node)
    {
        var methodName = node.Identifier.Text;
        var fullName = $"{_currentNamespace}.{_currentClass}.{methodName}";

        // Build parameters: new AutoLogParameter("a", typeof(int), a), ...
        var parameterInitializers = node.ParameterList.Parameters
            .Select(p =>
                SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName("AutoLogParameter"))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                            {
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(p.Identifier.Text))),

                                SyntaxFactory.Argument(
                                    SyntaxFactory.TypeOfExpression(p.Type)),

                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(p.Identifier.Text))
                            }))))
            .ToArray();

        // new AutoLogParameter[] { ...parameters... }
        var parametersArray =
            SyntaxFactory.ArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                            SyntaxFactory.IdentifierName("AutoLogParameter"))
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            parameterInitializers.Select(p => (ExpressionSyntax)p))));

        // new AutoLogMetadata(methodName, fullName, parameters, depth, instanceId, callPath)
        var metadataInit =
            SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("AutoLogMetadata"))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                        {
                            // methodName
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(methodName))),

                            // fullName
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(fullName))),

                            // parameters
                            SyntaxFactory.Argument(parametersArray),

                            // depth: AutoLoggerContext.CurrentDepth
                            SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("AutoLoggerContext"),
                                    SyntaxFactory.IdentifierName("CurrentDepth"))),

                            // instanceId: Guid.NewGuid()
                            SyntaxFactory.Argument(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Guid"),
                                        SyntaxFactory.IdentifierName("NewGuid")))),

                            // callPath: fullName (for now)
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(fullName)))
                        })));

        // var logger = new AutoLogger(metadataInit);
        var loggerInit =
            SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("logger"))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName("AutoLogger"))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(metadataInit))))))));

        // using (var logger = new AutoLogger(...)) { try { ORIGINAL BODY } catch (Exception ex) { logger.LogException(ex); throw; } }
        var usingStatement =
            SyntaxFactory.UsingStatement(
                    SyntaxFactory.Block(
                        BuildTryCatchBlock(node.Body)))
                .WithDeclaration(loggerInit);

        //return SyntaxFactory.Block(usingStatement);

        var block =
            SyntaxFactory.Block(usingStatement)
                .WithAdditionalAnnotations(Formatter.Annotation);

        return block;
    }

    private static TryStatementSyntax BuildTryCatchBlock(BlockSyntax originalBody)
    {
        var tryBlock = SyntaxFactory.Block(originalBody.Statements);

        var catchClause =
            SyntaxFactory.CatchClause()
                .WithDeclaration(
                    SyntaxFactory.CatchDeclaration(
                        SyntaxFactory.IdentifierName("Exception"),
                        SyntaxFactory.Identifier("ex")))
                .WithBlock(
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("logger.LogException(ex);"),
                        SyntaxFactory.ParseStatement("throw;")));

        return SyntaxFactory.TryStatement(
            tryBlock,
            SyntaxFactory.SingletonList(catchClause),
            null);
    }

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        //var hasUsing = node.Usings
        //    .Any(u => u.Name.ToString() == "Sentinel.Diagnostics.AutoLogRuntime");
        var hasUsing = node.Usings
            .Any(u => u.Name.ToString() == "Sentinel.Diagnostics.Core.Attributes");

        var updated = hasUsing
            ? node
            : node.AddUsings(
                SyntaxFactory.UsingDirective(
                    // We add "Sentinel.Diagnostics.AutoLogRuntime" to the UsingDirective, with a leading space for formatting
                    //SyntaxFactory.IdentifierName("Sentinel.Diagnostics.AutoLogRuntime")
                    SyntaxFactory.IdentifierName("Sentinel.Diagnostics.Core.Attributes")
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticWhitespace(" ")))
                    )
                    // Add a trailing newline after the using directive for formatting
                    .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed)));

        return base.VisitCompilationUnit(updated);
    }
}

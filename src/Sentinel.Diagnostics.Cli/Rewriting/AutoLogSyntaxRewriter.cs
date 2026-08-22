using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Sentinel.Diagnostics.Core.Metadata;
using System.Diagnostics;

namespace Sentinel.Diagnostics.Cli.Rewriting;

public sealed class AutoLogSyntaxRewriter : CSharpSyntaxRewriter
{
    private string _currentNamespace = string.Empty;
    private string _currentClass = string.Empty;

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        //Debug.WriteLine($"Namespace: {node.ToFullString().Replace("\r\n", Environment.NewLine)}");
        _currentNamespace = node.Name.ToString();
        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        //Debug.WriteLine($"FileScopedNamespace: {node.ToFullString().Replace("\r\n", Environment.NewLine)}");
        _currentNamespace = node.Name.ToString();
        return base.VisitFileScopedNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        //Debug.WriteLine($"Class: {node.ToFullString().Replace("\r\n", Environment.NewLine)}");
        _currentClass = node.Identifier.Text;
        return base.VisitClassDeclaration(node);
    }

    private StatementSyntax RewriteConsoleOrDebugWriteLine(StatementSyntax stmt)
    {
        if (stmt is not ExpressionStatementSyntax exprStmt)
            return stmt;

        if (exprStmt.Expression is not InvocationExpressionSyntax invoke)
            return stmt;

        if (invoke.Expression is not MemberAccessExpressionSyntax member)
            return stmt;

        var target = member.Expression.ToString();   // "Console" or "Debug"
        var method = member.Name.Identifier.Text;    // "WriteLine"

        if (method != "WriteLine")
            return stmt;

        // Only rewrite Console.WriteLine or Debug.WriteLine
        if (target != "Console" && target != "Debug")
            return stmt;

        // Extract argument
        var arg = invoke.ArgumentList.Arguments.FirstOrDefault();
        if (arg == null)
            return stmt;

        // Determine log level based on string literal prefix
        string logMethod = "Info"; // default
        string? textPrefix = null;

        if (arg.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var text = literal.Token.ValueText;

            textPrefix = literal.Token.ValueText;
        }
        else if (arg.Expression is InterpolatedStringExpressionSyntax interpolated && interpolated.IsKind(SyntaxKind.InterpolatedStringExpression))
        {
            // Interpolated string: $"INFO: something {value}"
            var firstTextPart = interpolated.Contents
                .OfType<InterpolatedStringTextSyntax>()
                .FirstOrDefault();

            if (firstTextPart != null)
                textPrefix = firstTextPart.TextToken.ValueText;
        }
        if (!string.IsNullOrEmpty(textPrefix))
        {
            if (textPrefix.StartsWith("INFO", StringComparison.OrdinalIgnoreCase) ||
                textPrefix.StartsWith("INFORMATION", StringComparison.OrdinalIgnoreCase))
                logMethod = "Info";
            else if (textPrefix.StartsWith("WARN", StringComparison.OrdinalIgnoreCase) ||
                     textPrefix.StartsWith("WARNING", StringComparison.OrdinalIgnoreCase))
                logMethod = "Warn";
            else if (textPrefix.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                logMethod = "Error";
            else if (textPrefix.StartsWith("CRITICAL", StringComparison.OrdinalIgnoreCase))
                logMethod = "Critical";
            else if (textPrefix.StartsWith("TRACE", StringComparison.OrdinalIgnoreCase))
                logMethod = "Trace";
        }
        else if (arg.Expression is IdentifierNameSyntax idName &&
                 idName.Identifier.Text == "ex")
        {
            // Console.WriteLine(ex) → logger.Error(ex)
            logMethod = "Error";
        }

        // Build: logger.<method>(arg)
        var loggerCall =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("logger"),
                        SyntaxFactory.IdentifierName(logMethod)))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(arg))));

        return loggerCall;
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        //Debug.WriteLine($"Method: {node.ToFullString().Replace("\r\n", Environment.NewLine)}");
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
    }

    private static bool HasAutoLogAttribute(MethodDeclarationSyntax node)
    {
        return node.AttributeLists
            .SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString() == "AutoLog");
    }

    private BlockSyntax BuildAutoLoggerWrappedBody(MethodDeclarationSyntax node)
    {
        Debug.WriteLine("=== BuildAutoLoggerWrappedBody START ===");
        Debug.WriteLine(node.NormalizeWhitespace().ToFullString());

        var methodName = node.Identifier.Text;
        var fullName = $"{_currentNamespace}.{_currentClass}.{methodName}";
        var methodType = GetMethodType(node);

        //
        // 1. Find or build logger initialization
        //
        var existingLoggerInit = FindExistingLoggerInit(node);

        VariableDeclarationSyntax loggerInit =
            existingLoggerInit ?? BuildLoggerInit(
                BuildMetadataInit(methodName, fullName, methodType, BuildParametersArray(node)));

        //
        // 2. Find existing try/catch/finally
        //
        var existingTry = FindExistingTry(node);

        TryStatementSyntax tryBlock;

        if (existingTry != null)
        {
            // Fix missing catch/finally/pop
            tryBlock = RepairTryStatement(existingTry);
        }
        else
        {
            // Build new try/catch/finally
            tryBlock = BuildTryCatchFinallyIfMissing(node.Body);
        }

        //
        // 3. Find or build using(logger)
        //
        var existingUsing = FindExistingUsing(node);

        UsingStatementSyntax usingStatement;
        if (existingUsing != null)
        {
            usingStatement = ReplaceTryInsideUsing(existingUsing, tryBlock);
        }
        else
        {
            usingStatement = WrapTryInUsing(loggerInit, tryBlock);
        }

        //
        // 4. If operation push exists → preserve original statements
        //
        if (HasOperationPush(node))
        {
            var originalStatements = node.Body.Statements;

            // Replace or insert using block
            var updatedStatements = ReplaceOrInsertUsing(originalStatements, usingStatement);

            return SyntaxFactory.Block(updatedStatements)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        //
        // 5. Add operation push (missing)
        //
        var parentInit = SyntaxFactory.ParseStatement(
            "var parent = SentinelOperationContext.CurrentOperationId;");

        var opInit = SyntaxFactory.ParseStatement(
            "var op = Guid.NewGuid();");

        var setOp = AddBlankLineAfter(
            SyntaxFactory.ParseStatement("SentinelOperationContext.CurrentOperationId = op;"));

        //
        // 6. Final block
        //
        var ret = SyntaxFactory.Block(parentInit, opInit, setOp, usingStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        Debug.WriteLine("=== BuildAutoLoggerWrappedBody END ===");
        return ret;
    }

    private TryStatementSyntax BuildTryCatchFinallyIfMissing(BlockSyntax originalBody)
    {
        bool hasTry = originalBody.Statements.Any(s => s is TryStatementSyntax);

        if (hasTry)
        {
            var existingTry = (TryStatementSyntax)originalBody.Statements
                .First(s => s is TryStatementSyntax);

            // Add finally if missing
            if (existingTry.Finally == null)
            {
                existingTry = existingTry.WithFinally(BuildOperationPopFinally());
            }

            // Add catch logging if missing
            if (!existingTry.Catches.Any())
            {
                existingTry = existingTry.WithCatches(
                    SyntaxFactory.SingletonList(BuildCatchClause()));
            }

            return existingTry;
        }

        // Build new try/catch/finally
        return SyntaxFactory.TryStatement(
            SyntaxFactory.Block(originalBody.Statements),
            SyntaxFactory.SingletonList(BuildCatchClause()),
            BuildOperationPopFinally());
    }

    private FinallyClauseSyntax BuildOperationPopFinally()
    {
        return SyntaxFactory.FinallyClause(
            SyntaxFactory.Block(
                SyntaxFactory.ParseStatement(
                    "SentinelOperationContext.CurrentOperationId = parent;")));
    }

    private ExpressionSyntax BuildParametersArray(MethodDeclarationSyntax node)
    {
        if (!node.ParameterList.Parameters.Any())
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Array"),
                    SyntaxFactory.GenericName("Empty")
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName("AutoLogParameter"))))));
        }

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
                            })))).ToArray();

        return SyntaxFactory.ArrayCreationExpression(
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
    }

    private ObjectCreationExpressionSyntax BuildMetadataInit(
        string methodName,
        string fullName,
        SentinelMethodType methodType,
        ExpressionSyntax parametersArray)
    {
        return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("AutoLogMetadata"))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                    {
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(methodName))),

                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(fullName))),

                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(methodType.ToString()))),

                    SyntaxFactory.Argument(parametersArray),

                    SyntaxFactory.Argument(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("AutoLoggerContext"),
                            SyntaxFactory.IdentifierName("CurrentDepth"))),

                    SyntaxFactory.Argument(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("Guid"),
                                SyntaxFactory.IdentifierName("NewGuid")))),

                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(fullName)))
                    })));
    }

    private CatchClauseSyntax BuildCatchClause()
    {
        return SyntaxFactory.CatchClause()
            .WithDeclaration(
                SyntaxFactory.CatchDeclaration(
                    SyntaxFactory.IdentifierName("Exception"),
                    SyntaxFactory.Identifier("ex")))
            .WithBlock(
                SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("logger.LogException(ex);"),
                    SyntaxFactory.ParseStatement("throw;")));
    }

    private FinallyClauseSyntax BuildFinallyClause()
    {
        return SyntaxFactory.FinallyClause(
            SyntaxFactory.Block(
                SyntaxFactory.ParseStatement(
                    "SentinelOperationContext.CurrentOperationId = parent;")));
    }

    private VariableDeclarationSyntax BuildLoggerInit(ObjectCreationExpressionSyntax metadataInit)
    {
        return SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("var"))
            .WithVariables(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator("logger")
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.IdentifierName("AutoLogger"))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList(new[]
                                            {
                                            SyntaxFactory.Argument(metadataInit),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("parent")),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("op"))
                                            })))))));
    }

    private UsingStatementSyntax WrapTryInUsing(
        VariableDeclarationSyntax loggerInit,
        TryStatementSyntax tryBlock)
    {
        Debug.WriteLine("=== WrapTryInUsing ===");
        Debug.WriteLine($"LOGGER INIT: {loggerInit.NormalizeWhitespace().ToFullString()}");
        Debug.WriteLine($"TRY BLOCK: {tryBlock.NormalizeWhitespace().ToFullString()}");

        var usingStmt = SyntaxFactory.UsingStatement(
            declaration: loggerInit,
            expression: null,
            statement: SyntaxFactory.Block(tryBlock));

        Debug.WriteLine($"USING RESULT: {usingStmt.NormalizeWhitespace().ToFullString()}");

        return usingStmt;
    }


    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        //Debug.WriteLine($"CompilationUnit: {node.ToFullString().Replace("\r\n", Environment.NewLine)}");
        var hasUsing = node.Usings
            .Any(u => u.Name.ToString() == "Sentinel.Diagnostics.Core.Attributes");

        var updated = hasUsing
            ? node
            : node.AddUsings(
                SyntaxFactory.UsingDirective(
                    // We add "Sentinel.Diagnostics.AutoLogRuntime" to the UsingDirective, with a leading space for formatting
                    SyntaxFactory.IdentifierName("Sentinel.Diagnostics.Core.Attributes")
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticWhitespace(" ")))
                    )
                    // Add a trailing newline after the using directive for formatting
                    .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.ElasticCarriageReturnLineFeed)));

        return base.VisitCompilationUnit(updated);
    }

    public static SentinelMethodType GetMethodType(SyntaxNode node)
    {
        return node switch
        {
            MethodDeclarationSyntax => SentinelMethodType.Method,
            ConstructorDeclarationSyntax => SentinelMethodType.Constructor,
            OperatorDeclarationSyntax => SentinelMethodType.Operator,
            ConversionOperatorDeclarationSyntax => SentinelMethodType.Operator,
            LocalFunctionStatementSyntax => SentinelMethodType.LocalFunction,
            LambdaExpressionSyntax => SentinelMethodType.Lambda,
            AnonymousMethodExpressionSyntax => SentinelMethodType.Lambda,

            AccessorDeclarationSyntax accessor => accessor.Kind() switch
            {
                SyntaxKind.GetAccessorDeclaration => SentinelMethodType.PropertyGetter,
                SyntaxKind.SetAccessorDeclaration => SentinelMethodType.PropertySetter,
                _ => SentinelMethodType.Method
            },

            _ => SentinelMethodType.Method
        };
    }

    private bool HasOperationPush(MethodDeclarationSyntax methodNode)
    {
        return methodNode.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .Any(IsParentOperationDeclaration);
    }

    private bool IsParentOperationDeclaration(LocalDeclarationStatementSyntax localDecl)
    {
        // Must be: var parent = <something>;
        if (!localDecl.Declaration.Variables.Any(v => v.Identifier.Text == "parent"))
            return false;

        var variable = localDecl.Declaration.Variables
            .First(v => v.Identifier.Text == "parent");

        // Must have an initializer
        if (variable.Initializer == null)
            return false;

        // Must be: SentinelOperationContext.CurrentOperationId
        return variable.Initializer.Value is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Expression is IdentifierNameSyntax id &&
               id.Identifier.Text == "SentinelOperationContext" &&
               memberAccess.Name.Identifier.Text == "CurrentOperationId";
    }

    private VariableDeclarationSyntax? FindExistingLoggerInit(MethodDeclarationSyntax methodNode)
    {
        Debug.WriteLine("=== FindExistingLoggerInit ===");

        // 1. Search local declarations
        foreach (var localDecl in methodNode.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            Debug.WriteLine($"LOCAL DECL: {localDecl.NormalizeWhitespace().ToFullString()}");

            var variableDecl = localDecl.Declaration;

            foreach (var v in variableDecl.Variables)
                Debug.WriteLine($"  VAR: {v.Identifier.Text}");

            var loggerVar = variableDecl.Variables.FirstOrDefault(v => v.Identifier.Text == "logger");
            if (loggerVar == null)
                continue;

            Debug.WriteLine("  FOUND logger variable in LOCAL DECL");

            if (loggerVar.Initializer?.Value is ObjectCreationExpressionSyntax objCreate)
            {
                Debug.WriteLine($"  INIT TYPE: {objCreate.Type}");
                if (objCreate.Type is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AutoLogger")
                {
                    Debug.WriteLine("  MATCH: AutoLogger init found in LOCAL DECL");
                    return variableDecl;
                }
            }
        }

        // 2. Search using‑statement declarations
        foreach (var usingStmt in methodNode.DescendantNodes().OfType<UsingStatementSyntax>())
        {
            Debug.WriteLine($"USING STMT: {usingStmt.NormalizeWhitespace().ToFullString()}");

            var decl = usingStmt.Declaration;
            if (decl == null)
            {
                Debug.WriteLine("  USING has no declaration");
                continue;
            }

            Debug.WriteLine($"  USING DECL: {decl.NormalizeWhitespace().ToFullString()}");

            var loggerVar = decl.Variables.FirstOrDefault(v => v.Identifier.Text == "logger");
            if (loggerVar == null)
                continue;

            Debug.WriteLine("  FOUND logger variable in USING DECL");

            if (loggerVar.Initializer?.Value is ObjectCreationExpressionSyntax objCreate)
            {
                Debug.WriteLine($"  INIT TYPE: {objCreate.Type}");
                if (objCreate.Type is IdentifierNameSyntax id &&
                    id.Identifier.Text == "AutoLogger")
                {
                    Debug.WriteLine("  MATCH: AutoLogger init found in USING DECL");
                    return decl;
                }
            }
        }

        Debug.WriteLine("=== NO EXISTING LOGGER INIT FOUND ===");
        return null;
    }

    private TryStatementSyntax? FindExistingTry(MethodDeclarationSyntax node)
    {
        Debug.WriteLine("=== FindExistingTry ===");

        // Search ANYWHERE inside the method body
        foreach (var tryStmt in node.DescendantNodes().OfType<TryStatementSyntax>())
        {
            Debug.WriteLine($"FOUND TRY: {tryStmt.NormalizeWhitespace().ToFullString()}");
            return tryStmt;
        }

        Debug.WriteLine("=== NO TRY FOUND ===");
        return null;
    }

    private UsingStatementSyntax? FindExistingUsing(MethodDeclarationSyntax methodNode)
    {
        // Look for: using (var logger = new AutoLogger(...)) { ... }
        foreach (var usingStmt in methodNode.DescendantNodes().OfType<UsingStatementSyntax>())
        {
            var decl = usingStmt.Declaration;
            if (decl == null)
                continue;

            // Must contain a variable named "logger"
            var loggerVar = decl.Variables.FirstOrDefault(v => v.Identifier.Text == "logger");
            if (loggerVar == null)
                continue;

            // Must have initializer
            if (loggerVar.Initializer?.Value is not ObjectCreationExpressionSyntax objCreate)
                continue;

            // Must be: new AutoLogger(...)
            if (objCreate.Type is IdentifierNameSyntax id &&
                id.Identifier.Text == "AutoLogger")
            {
                return usingStmt; // FOUND the existing using block
            }
        }

        return null; // No using block found
    }

    private CatchClauseSyntax RepairCatchClause(CatchClauseSyntax catchClause)
    {
        Debug.WriteLine("=== RepairCatchClause ===");
        Debug.WriteLine($"CATCH BEFORE: {catchClause.NormalizeWhitespace().ToFullString()}");

        //
        // STEP 1 — Rewrite Console.WriteLine / Debug.WriteLine FIRST
        //
        var rewrittenStatements = SyntaxFactory.List(
            catchClause.Block.Statements.Select(RewriteConsoleOrDebugWriteLine));

        //
        // STEP 2 — Check if logger.LogException(ex) already exists AFTER rewrite
        //
        bool hasLog =
            rewrittenStatements.OfType<ExpressionStatementSyntax>()
                .Any(stmt =>
                    stmt.Expression is InvocationExpressionSyntax invoke &&
                    invoke.Expression is MemberAccessExpressionSyntax member &&
                    member.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "logger" &&
                    member.Name.Identifier.Text == "LogException");

        Debug.WriteLine($"  Has logger.LogException? {hasLog}");

        //
        // STEP 3 — If already present, return rewritten block (no insertion)
        //
        if (hasLog)
        {
            Debug.WriteLine("  logger.LogException(ex) already present — no insertion needed");
            return catchClause.WithBlock(
                catchClause.Block.WithStatements(rewrittenStatements));
        }

        //
        // STEP 4 — Insert logger.LogException(ex) BEFORE throw
        //
        Debug.WriteLine("  ADDING logger.LogException(ex) BEFORE throw;");

        var logStmt = SyntaxFactory.ParseStatement("logger.LogException(ex);");

        var throwStmt = rewrittenStatements
            .OfType<ThrowStatementSyntax>()
            .FirstOrDefault();

        SyntaxList<StatementSyntax> finalStatements;

        if (throwStmt != null)
        {
            finalStatements = rewrittenStatements.Insert(
                rewrittenStatements.IndexOf(throwStmt), logStmt);
        }
        else
        {
            finalStatements = rewrittenStatements.Add(logStmt);
        }

        var updated = catchClause.WithBlock(
            catchClause.Block.WithStatements(finalStatements));

        Debug.WriteLine($"CATCH AFTER: {updated.NormalizeWhitespace().ToFullString()}");

        return updated;
    }
   
    private FinallyClauseSyntax RepairFinallyClause(FinallyClauseSyntax finallyClause)
    {
        Debug.WriteLine("=== RepairFinallyClause ===");
        Debug.WriteLine($"FINALLY BEFORE: {finallyClause.NormalizeWhitespace().ToFullString()}");

        bool hasPop =
            finallyClause.Block.Statements
                .OfType<ExpressionStatementSyntax>()
                .Any(stmt =>
                    stmt.Expression is AssignmentExpressionSyntax assign &&
                    assign.Left is MemberAccessExpressionSyntax left &&
                    left.Expression is IdentifierNameSyntax id &&
                    id.Identifier.Text == "SentinelOperationContext" &&
                    left.Name.Identifier.Text == "CurrentOperationId" &&
                    assign.Right is IdentifierNameSyntax right &&
                    right.Identifier.Text == "parent");

        Debug.WriteLine($"  Has operation-pop? {hasPop}");

        if (hasPop)
            return finallyClause;

        Debug.WriteLine("  ADDING SentinelOperationContext.CurrentOperationId = parent;");

        var popStmt = SyntaxFactory.ParseStatement(
            "SentinelOperationContext.CurrentOperationId = parent;");

        var updated = finallyClause.WithBlock(
            finallyClause.Block.AddStatements(popStmt));

        Debug.WriteLine($"FINALLY AFTER: {updated.NormalizeWhitespace().ToFullString()}");

        return updated;
    }

    private UsingStatementSyntax ReplaceTryInsideUsing(
        UsingStatementSyntax existingUsing,
        TryStatementSyntax repairedTry)
    {
        var innerBlock = existingUsing.Statement as BlockSyntax;

        var updatedBlock = innerBlock.ReplaceNode(
            innerBlock.Statements.OfType<TryStatementSyntax>().First(),
            repairedTry);

        return existingUsing.WithStatement(updatedBlock);
    }

    private TryStatementSyntax RepairTryStatement(TryStatementSyntax tryStmt)
    {
        Debug.WriteLine("=== RepairTryStatement ===");
        Debug.WriteLine($"TRY BEFORE: {tryStmt.NormalizeWhitespace().ToFullString()}");

        var repairedCatches = tryStmt.Catches.Count == 0
            ? SyntaxFactory.SingletonList(BuildCatchClause())
            : SyntaxFactory.List(tryStmt.Catches.Select(RepairCatchClause));

        var repairedFinally = tryStmt.Finally == null
            ? BuildFinallyClause()
            : RepairFinallyClause(tryStmt.Finally);

        var rewrittenTryBlock = tryStmt.Block.WithStatements(
            SyntaxFactory.List(
                tryStmt.Block.Statements.Select(RewriteConsoleOrDebugWriteLine)));

        var updated = SyntaxFactory.TryStatement(
            rewrittenTryBlock,
            repairedCatches,
            repairedFinally);

        Debug.WriteLine($"TRY AFTER: {updated.NormalizeWhitespace().ToFullString()}");

        return updated;
    }

    private SyntaxList<StatementSyntax> ReplaceOrInsertUsing(
        SyntaxList<StatementSyntax> originalStatements,
        UsingStatementSyntax newUsing)
    {
        Debug.WriteLine("=== ReplaceOrInsertUsing ===");

        foreach (var stmt in originalStatements)
            Debug.WriteLine($"ORIGINAL STMT: {stmt.NormalizeWhitespace().ToFullString()}");

        var existingUsing = originalStatements
            .OfType<UsingStatementSyntax>()
            .FirstOrDefault(u =>
                u.Declaration?.Variables.Any(v => v.Identifier.Text == "logger") == true);

        if (existingUsing != null)
        {
            Debug.WriteLine("  FOUND EXISTING USING(logger)");
            Debug.WriteLine($"  EXISTING: {existingUsing.NormalizeWhitespace().ToFullString()}");
            Debug.WriteLine($"  REPLACEMENT: {newUsing.NormalizeWhitespace().ToFullString()}");

            return originalStatements.Replace(existingUsing, newUsing);
        }

        Debug.WriteLine("  NO EXISTING USING(logger) — ADDING NEW ONE");

        return originalStatements.Add(newUsing);
    }

    private static T AddBlankLineAfter<T>(T statement) where T : SyntaxNode
    {
        return statement.WithTrailingTrivia(
            SyntaxFactory.TriviaList(
                SyntaxFactory.ElasticCarriageReturnLineFeed,
                SyntaxFactory.ElasticCarriageReturnLineFeed));
    }
}

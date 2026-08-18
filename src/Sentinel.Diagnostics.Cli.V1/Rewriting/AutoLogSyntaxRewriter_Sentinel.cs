using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sentinel.Diagnostics.Cli.Configuration;

namespace Sentinel.Diagnostics.Cli.Rewriting;

public sealed class AutoLogSyntaxRewriter_Sentinel(string methodName, AutoLogSection config) : CSharpSyntaxRewriter
{
    private readonly string _methodName = methodName;
    private readonly AutoLogSection _config = config;

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Expression-bodied method?
        if (node.ExpressionBody != null)
            return RewriteExpressionBodiedMethod(node);
        
        if (node.Identifier.Text != _methodName)
            return node;

        if (node.Body == null)
            return node;

        bool isAsync = node.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));

        bool returnsTask =
            node.ReturnType is IdentifierNameSyntax id &&
            (id.Identifier.Text == "Task" || id.Identifier.Text == "ValueTask");

        bool returnsGenericTask =
            node.ReturnType is GenericNameSyntax gen &&
            (gen.Identifier.Text == "Task" || gen.Identifier.Text == "ValueTask");

        bool isAsyncReturn = returnsTask || returnsGenericTask;

        var statements = new List<StatementSyntax>();

        // ------------------------------------------------------------
        // PARAMETER LOGGING
        // ------------------------------------------------------------
        if (_config.LogParameters)
        {
            foreach (var p in node.ParameterList.Parameters)
            {
                statements.Add(
                    SyntaxFactory.ParseStatement(
                        InstrumentationTemplates.LogParameter(p.Identifier.Text)));
            }
        }

        // ------------------------------------------------------------
        // DURATION START
        // ------------------------------------------------------------
        if (_config.LogDuration)
        {
            statements.Add(
                SyntaxFactory.ParseStatement(
                    InstrumentationTemplates.DurationStart()));
        }

        // ------------------------------------------------------------
        // ENTER LOGGING
        // ------------------------------------------------------------
        statements.Add(
            SyntaxFactory.ParseStatement(
                InstrumentationTemplates.LogEnter(_methodName)));

        // ------------------------------------------------------------
        // ORIGINAL BODY
        // ------------------------------------------------------------
        var originalStatements = node.Body.Statements
            .Select(stmt =>
            {
                if (stmt is ReturnStatementSyntax retStmt)
                    return RewriteReturn(retStmt);

                return stmt;
            })
            .ToList();


        // ------------------------------------------------------------
        // RETRY LOGIC (async-aware)
        // ------------------------------------------------------------
        BlockSyntax tryBlock;

        if (_config.RetryCount > 0)
        {
            var retryLines = InstrumentationTemplates.RetryStatements(
                _methodName,
                _config.RetryCount,
                _config.RetryDelayMilliseconds);

            var retryStatements = retryLines
                .Select(l => SyntaxFactory.ParseStatement(
                    isAsync ? l.Replace("Thread.Sleep", "await Task.Delay") : l))
                .ToList();

            var index = retryStatements.FindIndex(s =>
                s.ToString().Contains("__SENTINEL_ORIGINAL_BODY__"));

            retryStatements.RemoveAt(index);
            retryStatements.InsertRange(index, originalStatements);

            tryBlock = SyntaxFactory.Block(retryStatements);
        }
        else
        {
            tryBlock = SyntaxFactory.Block(originalStatements);
        }

        // ------------------------------------------------------------
        // CATCH BLOCK
        // ------------------------------------------------------------
        var catchClause =
            SyntaxFactory.CatchClause()
                .WithDeclaration(
                    SyntaxFactory.CatchDeclaration(
                        SyntaxFactory.IdentifierName("Exception"),
                        SyntaxFactory.Identifier("ex")))
                .WithBlock(
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement(
                            InstrumentationTemplates.LogException(_methodName)),
                        SyntaxFactory.ParseStatement("throw;")));

        // ------------------------------------------------------------
        // FINALLY BLOCK
        // ------------------------------------------------------------
        var finallyStatements = new List<StatementSyntax>();

        if (_config.LogDuration)
        {
            finallyStatements.Add(
                SyntaxFactory.ParseStatement(
                    InstrumentationTemplates.DurationEnd(_methodName)));
        }

        finallyStatements.Add(
            SyntaxFactory.ParseStatement(
                InstrumentationTemplates.LogExit(_methodName)));

        var finallyClause =
            SyntaxFactory.FinallyClause(
                SyntaxFactory.Block(finallyStatements));

        // ------------------------------------------------------------
        // ASYNC-SAFE WRAPPER
        // ------------------------------------------------------------
        var tryStatement =
            SyntaxFactory.TryStatement(
                tryBlock,
                SyntaxFactory.SingletonList(catchClause),
                finallyClause);

        var wrappedBody =
            SyntaxFactory.Block(
                statements.Concat([tryStatement]));

        return node.WithBody(wrappedBody);
    }

    private BlockSyntax RewriteReturn(ReturnStatementSyntax returnStmt)
    {
        // Extract the return expression
        var expr = returnStmt.Expression;

        // Build: var __sentinelReturn = <expr>;
        var assign = SyntaxFactory.ParseStatement(
            $"var __sentinelReturn = {expr};");

        // Build: SentinelLogger.Return("<method>", __sentinelReturn);
        var log = SyntaxFactory.ParseStatement(
            $"SentinelLogger.Return(\"{_methodName}\", __sentinelReturn);");

        // Build: return __sentinelReturn;
        var ret = SyntaxFactory.ReturnStatement(
            SyntaxFactory.IdentifierName("__sentinelReturn"));

        return SyntaxFactory.Block(assign, log, ret);
    }

    private MethodDeclarationSyntax RewriteExpressionBodiedMethod(MethodDeclarationSyntax node)
    {
        var expr = node.ExpressionBody!.Expression;

        // Convert expression-bodied to block-bodied
        // Build: var __sentinelReturn = <expr>;
        var assign = SyntaxFactory.ParseStatement(
            $"var __sentinelReturn = {expr};");

        // Build: SentinelLogger.Return("<method>", __sentinelReturn);
        var logReturn = SyntaxFactory.ParseStatement(
            $"SentinelLogger.Return(\"{_methodName}\", __sentinelReturn);");

        // Build: return __sentinelReturn;
        var ret = SyntaxFactory.ReturnStatement(
            SyntaxFactory.IdentifierName("__sentinelReturn"));

        // Build try block
        var tryBlock = SyntaxFactory.Block(assign, logReturn, ret);

        // Build catch block
        var catchClause =
            SyntaxFactory.CatchClause()
                .WithDeclaration(
                    SyntaxFactory.CatchDeclaration(
                        SyntaxFactory.IdentifierName("Exception"),
                        SyntaxFactory.Identifier("ex")))
                .WithBlock(
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement(
                            InstrumentationTemplates.LogException(_methodName)),
                        SyntaxFactory.ParseStatement("throw;")));

        // Build finally block
        var finallyStatements = new List<StatementSyntax>();

        if (_config.LogDuration)
        {
            finallyStatements.Add(
                SyntaxFactory.ParseStatement(
                    InstrumentationTemplates.DurationEnd(_methodName)));
        }

        finallyStatements.Add(
            SyntaxFactory.ParseStatement(
                InstrumentationTemplates.LogExit(_methodName)));

        var finallyClause =
            SyntaxFactory.FinallyClause(
                SyntaxFactory.Block(finallyStatements));

        // Build entry logging + duration + parameters
        var entryStatements = new List<StatementSyntax>();

        if (_config.LogParameters)
        {
            foreach (var p in node.ParameterList.Parameters)
            {
                entryStatements.Add(
                    SyntaxFactory.ParseStatement(
                        InstrumentationTemplates.LogParameter(p.Identifier.Text)));
            }
        }

        if (_config.LogDuration)
        {
            entryStatements.Add(
                SyntaxFactory.ParseStatement(
                    InstrumentationTemplates.DurationStart()));
        }

        entryStatements.Add(
            SyntaxFactory.ParseStatement(
                InstrumentationTemplates.LogEnter(_methodName)));

        // Build final block
        var newBody =
            SyntaxFactory.Block(
                entryStatements.Concat(new[]
                {
                SyntaxFactory.TryStatement(
                    tryBlock,
                    SyntaxFactory.SingletonList(catchClause),
                    finallyClause)
                }));

        // Remove expression body and replace with block body
        return node
            .WithExpressionBody(null)
            .WithSemicolonToken(default)
            .WithBody(newBody);
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Sentinel.Diagnostics.Cli.Configuration;
using Sentinel.Diagnostics.Cli.Scanning;

namespace Sentinel.Diagnostics.Cli.Rewriting;

public sealed class AutoLogRewriter(SentinelConfig config)
{
    private readonly SentinelConfig _config = config;

    public async Task RewriteProjectAsync(Project project)
    {
        var targets = await AutoLogScanner.ScanProjectAsync(project);

        foreach (var target in targets)
            await RewriteFileAsync(project, target);
    }

    private async Task RewriteAllFilesAsync(Project project, AutoLogScanResult target)
    {
        var rewriter =
            new AutoLogSyntaxRewriter();

        foreach (var document in project.Documents)
        {
            var root = await document.GetSyntaxRootAsync();
            var newRoot = rewriter.Visit(root);

            if (!ReferenceEquals(root, newRoot))
            {
                var formattedDoc = await Formatter.FormatAsync(
                    document.WithSyntaxRoot(newRoot),
                    Formatter.Annotation);

                var formattedRoot = await formattedDoc.GetSyntaxRootAsync();
                File.WriteAllText(document.FilePath, formattedRoot.ToFullString());
            }
        }
    }

    private async Task RewriteFileAsync_orig(Project project, AutoLogScanResult target)
    {
        var document = project.Documents.First(d =>
            d.FilePath != null &&
            d.FilePath.Equals(target.FilePath, StringComparison.OrdinalIgnoreCase));

        var root = await document.GetSyntaxRootAsync();

        // AddUsing injection
        root = InjectUsings(root, _config.AutoLog);

        var syntaxRewriter = new AutoLogSyntaxRewriter();

        var newRoot = syntaxRewriter.Visit(root);

        File.Copy(target.FilePath, target.FilePath + ".bak", overwrite: true);
        File.WriteAllText(target.FilePath, newRoot.ToFullString());
    }

    private async Task RewriteFileAsync(Project project, AutoLogScanResult target)
    {
        var document = project.Documents.First(d =>
            d.FilePath != null &&
            d.FilePath.Equals(target.FilePath, StringComparison.OrdinalIgnoreCase));

        // Force Roslyn to re-parse the file using correct project settings
        var updatedDoc = document.WithText(SourceText.From(File.ReadAllText(target.FilePath)));

        var root = await updatedDoc.GetSyntaxRootAsync();

        // AddUsing injection
        root = InjectUsings(root, _config.AutoLog);

        var syntaxRewriter = new AutoLogSyntaxRewriter();
        var newRoot = syntaxRewriter.Visit(root);

        File.Copy(target.FilePath, target.FilePath + ".bak", overwrite: true);
        File.WriteAllText(target.FilePath, newRoot.ToFullString());
    }

    private async Task RewriteFileAsync__Sentinel(Project project, AutoLogScanResult target)
    {
        var document = project.Documents.First(d =>
            d.FilePath != null &&
            d.FilePath.Equals(target.FilePath, StringComparison.OrdinalIgnoreCase));

        var root = await document.GetSyntaxRootAsync();

        // AddUsing injection
        root = InjectUsings(root, _config.AutoLog);

        var syntaxRewriter = new AutoLogSyntaxRewriter_Sentinel(
            target.MethodName,
            _config.AutoLog);

        var newRoot = syntaxRewriter.Visit(root);

        File.Copy(target.FilePath, target.FilePath + ".bak", overwrite: true);
        File.WriteAllText(target.FilePath, newRoot.ToFullString());
    }

    private static SyntaxNode InjectUsings(SyntaxNode root, AutoLogSection config)
    {
        if (!config.AddUsing)
            return root;

        const string requiredUsing = "Sentinel.Diagnostics";

        var compilationUnit = (CompilationUnitSyntax)root;

        // Already present?
        bool exists = compilationUnit.Usings
            .Any(u => u.Name?.ToString() == requiredUsing);

        if (exists)
            return root;

        // Create the using directive
        var newUsing = SyntaxFactory.UsingDirective(
            SyntaxFactory.ParseName(requiredUsing))
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        // Insert after existing usings
        var newCompilationUnit = compilationUnit.AddUsings(newUsing);

        return newCompilationUnit;
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Sentinel.Diagnostics.Cli.Scanning;

public sealed class AutoLogScanResult
{
    public string FilePath { get; init; } = string.Empty;
    public string MethodName { get; init; } = string.Empty;
    public string ContainingType { get; init; } = string.Empty;
}

public sealed class AutoLogScanner
{
    public static async Task<IReadOnlyList<AutoLogScanResult>> ScanProjectAsync(Project project)
    {
        var results = new List<AutoLogScanResult>();

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);

        //var updatedDocs = project.Documents
        //    .Where(d => !d.FilePath.Contains(@"\obj\"))
        //    .Select(d => d.WithParseOptions(parseOptions))
        //    .ToList();
        Console.WriteLine("=== DOCUMENTS IN PROJECT ===");
        foreach (var document in project.Documents)
        {
            Console.Write($"File: {document.FilePath}: ");
            
            if (!document.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ?? true)
                continue;

            // Force Roslyn to re-parse the file using correct project settings
            var updatedDoc = document.WithText(SourceText.From(File.ReadAllText(document.FilePath)));

            var root = await updatedDoc.GetSyntaxRootAsync();

            if (root is null)
                root = await document.GetSyntaxRootAsync();

            if (root is null)
            {
                Console.WriteLine($"root not detected.");
                continue;
            }
            Console.WriteLine($"root detected.");

            var semanticModel = await document.GetSemanticModelAsync();
            if (semanticModel is null)
                continue;

            // Find all method declarations
            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methodNodes)
            {
                if (!HasAutoLogAttribute(method, semanticModel))
                    continue;

                var containingType = method.Parent as TypeDeclarationSyntax;
                var typeName = containingType?.Identifier.Text ?? "<unknown>";

                results.Add(new AutoLogScanResult
                {
                    FilePath = document.FilePath!,
                    MethodName = method.Identifier.Text,
                    ContainingType = typeName
                });
            }
        }

        return results;
    }

    private static bool HasAutoLogAttribute(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel)
    {
        foreach (var attributeList in method.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                if (symbol?.ContainingType == null)
                    continue;

                var attributeName = symbol.ContainingType.Name;

                // Matches: AutoLog, AutoLogAttribute
                if (attributeName.Equals("AutoLog", StringComparison.Ordinal) ||
                    attributeName.Equals("AutoLogAttribute", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
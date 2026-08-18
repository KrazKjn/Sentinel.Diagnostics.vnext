using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Sentinel.Diagnostics.Cli.Workspace;

public sealed class SolutionInspector
{
    public async Task ScanAsync(Solution solution)
    {
        Console.WriteLine("=== Detailed Solution Inspection ===");

        foreach (var project in solution.Projects)
        {
            PrintProjectHeader(project);

            foreach (var document in project.Documents)
            {
                await PrintDocumentDetailsAsync(project, document);
            }

            Console.WriteLine();
        }
    }

    private void PrintProjectHeader(Microsoft.CodeAnalysis.Project project)
    {
        Console.WriteLine($"Project: {project.Name}");
        Console.WriteLine($"  File: {project.FilePath}");
        Console.WriteLine($"  Language: {project.Language}");
        Console.WriteLine($"  Documents: {project.Documents.Count()}");
        Console.WriteLine($"  ParseOptions: {project.ParseOptions}");
        var csharpOptions = project.ParseOptions as Microsoft.CodeAnalysis.CSharp.CSharpParseOptions;

        if (csharpOptions != null)
        {
            Console.WriteLine($"  ParseOptions: CSharpParseOptions(LanguageVersion = {csharpOptions.LanguageVersion})");
        }
        else
        {
            Console.WriteLine("  ParseOptions: (not C#)");
        }

        var msbuildProject = LoadMsbuildProject(project.FilePath);

        string tfm = msbuildProject.GetPropertyValue("TargetFramework");
        string tfms = msbuildProject.GetPropertyValue("TargetFrameworks");

        if (!string.IsNullOrWhiteSpace(tfm))
            Console.WriteLine($"  Target Framework: {tfm}");
        else if (!string.IsNullOrWhiteSpace(tfms))
            Console.WriteLine($"  Target Frameworks: {tfms}");
        else
        {
            tfm = TargetFrameworkResolver.ResolveFromOutputPath(project.OutputFilePath);
            if (!string.IsNullOrWhiteSpace(tfm))
                Console.WriteLine($"  Target Framework: {tfm}");
            else
                Console.WriteLine("  Target Framework: Unknown");
        }
        Console.WriteLine();
    }


    private static Microsoft.Build.Evaluation.Project LoadMsbuildProject(string projectPath)
    {
        var globalProps = new Dictionary<string, string>
        {
            { "Configuration", "Debug" },
            { "Platform", "AnyCPU" }
        };

        var collection = new ProjectCollection(globalProps);
        return collection.LoadProject(projectPath);
    }

    private async Task PrintDocumentDetailsAsync(Microsoft.CodeAnalysis.Project project, Document document)
    {
        var filePath = document.FilePath ?? "(no file path)";
        var isGenerated = SentinelWorkspace.IsGenerated(document);
        if (isGenerated)
            return; // Skip generated files for detailed inspection

        var kind = SentinelWorkspace.GetDocumentKind(document);

        var tree = await document.GetSyntaxTreeAsync();

        tree ??=
                CSharpSyntaxTree.ParseText(document.FilePath);

        var hasTree = tree != null;

        Console.WriteLine($"    Document: {filePath}");
        Console.WriteLine($"      Kind: {kind}");
        Console.WriteLine($"      Category: {GetCategory(document, isGenerated)}");
        Console.WriteLine($"      Generated: {isGenerated}");
        Console.WriteLine($"      HasTree: {hasTree}");

        if (hasTree)
        {
            var root = await document.GetSyntaxRootAsync();
            root ??= await tree.GetRootAsync();
            var text = await document.GetTextAsync();

            Console.WriteLine($"      Lines: {text.Lines.Count}");
            Console.WriteLine($"      Encoding: {GetEncodingName(text.Encoding)}");
            Console.WriteLine($"      FileSize: {GetFileSize(filePath)} bytes");

            var csharpOptions = project.ParseOptions as Microsoft.CodeAnalysis.CSharp.CSharpParseOptions;

            if (csharpOptions != null)
            {
                Console.WriteLine($"      LanguageVersion: {csharpOptions.LanguageVersion}");
                Console.WriteLine($"      PreprocessorSymbols: {string.Join(", ", csharpOptions.PreprocessorSymbolNames)}");
                Console.WriteLine($"      DocumentationMode: {csharpOptions.DocumentationMode}");
            }
            else
            {
                Console.WriteLine("      LanguageVersion: (not C#)");
            }
            Console.WriteLine($"      ContainsTopLevelStatements: {ContainsTopLevelStatements(root)}");
            Console.WriteLine($"      ContainsFileScopedNamespace: {ContainsFileScopedNamespace(root)}");
            Console.WriteLine($"      ContainsGlobalUsings: {ContainsGlobalUsings(root)}");
        }
        else
        {
            Console.WriteLine($"      Lines: (no tree)");
            Console.WriteLine($"      Encoding: (unknown)");
            Console.WriteLine($"      FileSize: {GetFileSize(filePath)} bytes");
        }

        Console.WriteLine();
    }

    // ----------------------------------------------------------------------
    // CLASSIFICATION HELPERS
    // ----------------------------------------------------------------------

    private string GetCategory(Document doc, bool isGenerated)
    {
        if (isGenerated)
            return "Generated";

        if (SentinelWorkspace.IsCodeFile(doc))
            return "Code";

        if (SentinelWorkspace.IsNonCodeFile(doc))
            return "Non-Code";

        if (doc.FilePath?.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) == true)
            return "Designer";

        return "Other";
    }

    private string GetEncodingName(Encoding? encoding)
    {
        if (encoding == null)
            return "Unknown";

        return encoding.EncodingName;
    }

    private long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;

        return new FileInfo(filePath).Length;
    }

    private bool ContainsTopLevelStatements(SyntaxNode root)
    {
        return root.ChildNodes().Any(n => n.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalStatement);
    }

    private bool ContainsFileScopedNamespace(SyntaxNode root)
    {
        return root.DescendantNodes().Any(n =>
            n.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.FileScopedNamespaceDeclaration);
    }

    private bool ContainsGlobalUsings(SyntaxNode root)
    {
        return root.DescendantNodes().Any(n =>
            n.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalStatement ||
            n.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.UsingDirective &&
            ((Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax)n).GlobalKeyword != default);
    }
}

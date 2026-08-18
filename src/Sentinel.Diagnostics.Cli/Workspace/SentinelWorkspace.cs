using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace Sentinel.Diagnostics.Cli.Workspace
{
    public sealed class SentinelWorkspace : IAsyncDisposable
    {
        private MSBuildWorkspace _workspace = null!;
        private bool _initialized;

        public async Task<Solution?> LoadSolutionAsync(string solutionPath)
        {
            if (!_initialized)
            {
                RegisterMSBuild("8.0.*");
                CreateWorkspace();
                _initialized = true;
            }

            Console.WriteLine($"Opening solution: {solutionPath}");
            var solution = await _workspace.OpenSolutionAsync(solutionPath);

            Console.WriteLine($"Loaded solution: {solution.FilePath}");
            Console.WriteLine($"Projects found: {solution.Projects.Count()}");

            LogProjectSummary(solution);

            return solution;
        }

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

        private void RegisterMSBuild(string sdkVersionPattern)
        {
            if (MSBuildLocator.IsRegistered)
                return;

            var instances = MSBuildLocator.QueryVisualStudioInstances()
                .Where(i => i.MSBuildPath.Contains("dotnet", StringComparison.OrdinalIgnoreCase)
                         || i.MSBuildPath.Contains("MSBuild", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!instances.Any())
                throw new InvalidOperationException("No MSBuild instances found.");

            // Simple heuristic: pick first matching instance
            var instance = instances.First();
            Console.WriteLine($"Using MSBuild from: {instance.MSBuildPath}");

            MSBuildLocator.RegisterInstance(instance);
        }

        private void CreateWorkspace()
        {
            _workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
            {
                ["ProvideCommandLineArgs"] = "true"
            });

            _workspace.RegisterWorkspaceFailedHandler(args =>
            {
                Console.WriteLine($"WORKSPACE ERROR: {args.Diagnostic.Kind}: {args.Diagnostic.Message}");
            });
        }

        private static void LogProjectSummary(Solution solution)
        {
            Console.WriteLine("=== Project Summary ===");
            foreach (var project in solution.Projects)
            {
                Console.WriteLine($"Project: {project.Name}");
                Console.WriteLine($"  FilePath: {project.FilePath}");
                Console.WriteLine($"  Language: {project.Language}");
                Console.WriteLine($"  Documents: {project.Documents.Count()}");

                var csharpOptions = project.ParseOptions as CSharpParseOptions;
                if (csharpOptions != null)
                {
                    Console.WriteLine($"  LanguageVersion: {csharpOptions.LanguageVersion}");
                    Console.WriteLine($"  PreprocessorSymbols: {string.Join(", ", csharpOptions.PreprocessorSymbolNames)}");
                    Console.WriteLine($"  DocumentationMode: {csharpOptions.DocumentationMode}");
                }

                Console.WriteLine();
            }
        }

        private static void PrintProjectHeader(Project project)
        {
            Console.WriteLine($"=== Project: {project.Name} ===");
            Console.WriteLine($"  FilePath: {project.FilePath}");
            Console.WriteLine($"  Language: {project.Language}");
            Console.WriteLine($"  Documents: {project.Documents.Count()}");
        }

        private async Task PrintDocumentDetailsAsync(Project project, Document document)
        {
            var filePath = document.FilePath ?? "(no file path)";
            var isGenerated = IsGenerated(document);
            if (isGenerated)
                return;

            var kind = GetDocumentKind(document);

            var tree = await document.GetSyntaxTreeAsync();
            if (tree == null && filePath != "(no file path)" && File.Exists(filePath))
            {
                var text = await File.ReadAllTextAsync(filePath);
                tree = CSharpSyntaxTree.ParseText(text, path: filePath);
            }

            var hasTree = tree != null;

            Console.WriteLine($"    Document: {filePath}");
            Console.WriteLine($"      Kind: {kind}");
            Console.WriteLine($"      Category: {GetCategory(document, isGenerated)}");
            Console.WriteLine($"      Generated: {isGenerated}");
            Console.WriteLine($"      HasTree: {hasTree}");

            if (hasTree)
            {
                var root = await document.GetSyntaxRootAsync();
                if (root == null && tree != null)
                    root = await tree.GetRootAsync();

                var text = await document.GetTextAsync();
                if (text == null && filePath != "(no file path)" && File.Exists(filePath))
                {
                    var raw = await File.ReadAllTextAsync(filePath);
                    text = SourceText.From(raw);
                }

                Console.WriteLine($"      Lines: {text?.Lines.Count ?? 0}");
                Console.WriteLine($"      Encoding: {GetEncodingName(text?.Encoding)}");
                Console.WriteLine($"      FileSize: {GetFileSize(filePath)} bytes");

                var csharpOptions = project.ParseOptions as CSharpParseOptions;
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

        public static string GetDocumentKind(Document doc)
        {
            return doc.SourceCodeKind switch
            {
                SourceCodeKind.Regular => "Code",
                SourceCodeKind.Script => "Script",
                _ => "Other"
            };
        }

        public static bool IsGenerated(Document doc)
        {
            var filePath = doc.FilePath;
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static string GetCategory(Document doc, bool isGenerated)
        {
            if (isGenerated)
                return "Generated";

            if (doc.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return "CSharp";

            return "Other";
        }

        private static string GetEncodingName(System.Text.Encoding? encoding)
        {
            return encoding?.WebName ?? "(unknown)";
        }

        private static long GetFileSize(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return 0;

            return new FileInfo(filePath).Length;
        }

        private static bool ContainsTopLevelStatements(SyntaxNode? root)
        {
            if (root is null) return false;
            return root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax>().Any();
        }

        private static bool ContainsFileScopedNamespace(SyntaxNode? root)
        {
            if (root is null) return false;
            return root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>().Any();
        }

        private static bool ContainsGlobalUsings(SyntaxNode? root)
        {
            if (root is null) return false;
            return root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
                .Any(u => u.GlobalKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalKeyword));
        }

        public static bool IsCodeFile(Document doc)
        {
            return doc.SourceCodeKind == SourceCodeKind.Regular &&
                   doc.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsNonCodeFile(Document doc)
        {
            return doc.SourceCodeKind != SourceCodeKind.Regular;
        }

        public async ValueTask DisposeAsync()
        {
            if (_workspace != null!)
            {
                _workspace.Dispose();
            }

            await Task.CompletedTask;
        }
    }
}

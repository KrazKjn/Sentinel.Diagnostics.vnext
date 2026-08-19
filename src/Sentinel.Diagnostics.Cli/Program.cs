using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Sentinel.Diagnostics.Cli.Configuration;
using Sentinel.Diagnostics.Cli.Rewriting;
using Sentinel.Diagnostics.Cli.Scanning;
using Sentinel.Diagnostics.Cli.Workspace;
namespace Sentinel.Diagnostics.Cli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        //MSBuildLocator.RegisterMSBuildPath(
        //    @"C:\Program Files\dotnet\sdk\8.0.424");

        var config = ProjectConfigurationLoader.LoadFromFile("sentinel.json");

        if (config.AutoLog is null)
        {
            Console.WriteLine("AutoLog section missing.");
        }
        else
        {
            Console.WriteLine($"Enabled: {config.AutoLog.Enabled}");
            Console.WriteLine($"AddUsing: {config.AutoLog.AddUsing}");
            Console.WriteLine($"AddTryCatch: {config.AutoLog.AddTryCatch}");
            Console.WriteLine($"LogParameters: {config.AutoLog.LogParameters}");
            Console.WriteLine($"LogDuration: {config.AutoLog.LogDuration}");
            Console.WriteLine($"RetryCount: {config.AutoLog.RetryCount}");
            Console.WriteLine($"RetryDelayMs: {config.AutoLog.RetryDelayMilliseconds}");
        }

        Console.WriteLine("Sentinel Diagnostics CLI");
        Console.WriteLine("-------------------------");

        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "rewrite":
                await RunRewrite(args);
                break;

            case "validate":
                await RunValidate(args);
                break;

            case "scan":
                await RunScan(args);
                break;

            case "init":
                await RunInit(args);
                break;

            case "updatecsharp":
                await UpdateCSharp(args);
                break;

            case "sentinel-cli":
                await RunInspectionAsync(args);
                break;

            default:
                Console.WriteLine($"Unknown command: {command}");
                PrintUsage();
                break;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  sentinel init <path>        Create default sentinel.json");
        Console.WriteLine("  sentinel validate <path>    Validate sentinel.json");
        Console.WriteLine("  sentinel scan <csproj>      Scan project for [AutoLog]");
        Console.WriteLine("  sentinel rewrite <csproj>   Rewrite files with AutoLog");
    }

    private static async Task RunInit(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing path for sentinel.json");
            return;
        }

        var path = args[1];
        await SentinelJsonWriter.CreateDefaultAsync(path);

        Console.WriteLine($"Created default sentinel.json at: {path}");
    }
    private static async Task RunValidate(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing sentinel.json path");
            return;
        }

        var jsonPath = args[1];
        var json = await File.ReadAllTextAsync(jsonPath);

        var config = ProjectConfigurationLoader.LoadFromFile(jsonPath);

        SentinelJsonValidator.Validate(json, config);

        if (config.AutoLog?.Diagnostics.Count == 0)
        {
            Console.WriteLine("sentinel.json is valid.");
        }
        else
        {
            Console.WriteLine("Diagnostics:");
            foreach (var diag in config.AutoLog!.Diagnostics)
                Console.WriteLine($"  {diag.Id}: {diag.Message}");
        }
    }

    private static async Task RunScan(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing .csproj path");
            return;
        }

        var projectPath = args[1];

        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath);

        var results = await AutoLogScanner.ScanProjectAsync(project);

        Console.WriteLine("AutoLog scan results:");
        foreach (var r in results)
            Console.WriteLine($"  {r.FilePath}: {r.MethodName}");
    }

    private static async Task RunRewrite(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: sentinel rewrite <solution> <csproj name> <sentinel.json>");
            return;
        }

        var solutionPath = args[1];
        var projectName = args[2];
        var configPath = args[3];

        //MSBuildLocator.RegisterDefaults();

        var config = ProjectConfigurationLoader.LoadFromFile(configPath);

        using var workspace = MSBuildWorkspace.Create();

        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            Console.WriteLine("WORKSPACE ERROR: " + args.Diagnostic);
        });


        var solution = await workspace.OpenSolutionAsync(solutionPath);
        var project = solution.Projects.First(p => p.Name == projectName);

        //var project = await workspace.OpenProjectAsync(projectPath);

        var rewriter = new AutoLogRewriter(config);
        await rewriter.RewriteProjectAsync(project);

        Console.WriteLine("Rewrite complete.");
    }

    private static async Task UpdateCSharp(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: sentinel updatecsharp <CSharp File>");
            return;
        }

        string csprojPath = args[1];
        var sourceText = await File.ReadAllTextAsync(csprojPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
        var root = syntaxTree.GetRoot();

        var rewriter = new AutoLogSyntaxRewriter();
        var newRoot = (CompilationUnitSyntax)rewriter.Visit(root);

        if (!ReferenceEquals(root, newRoot))
        {
            var newText = newRoot.ToFullString();
            //var workspace = new AdhocWorkspace();
            //var formattedRoot = Formatter.Format(
            //    newRoot,
            //    Formatter.Annotation,
            //    workspace);
            //var newText = formattedRoot.ToFullString();

            File.Copy(csprojPath, csprojPath + ".bak", overwrite: true);
            await File.WriteAllTextAsync(csprojPath, newText);
        }
    }

    private static async Task UpdateCSharp2(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: sentinel updatecsharp <CSharp File>");
            return;
        }

        string sourcePath = args[1];

        var sourceText =
            await File.ReadAllTextAsync(sourcePath);

        var syntaxTree =
            CSharpSyntaxTree.ParseText(sourceText);

        var root =
            syntaxTree.GetRoot();

        var rewriter =
            new AutoLogSyntaxRewriter();

        var newRoot =
            (CompilationUnitSyntax)rewriter.Visit(root)!;

        if (ReferenceEquals(root, newRoot))
            return;

        Console.WriteLine("===== BEFORE FORMAT =====");
        Console.WriteLine(newRoot.ToFullString());
        Console.WriteLine("=========================");

        Console.WriteLine(
            $"Generated formatter annotations: " +
            $"{newRoot.GetAnnotatedNodes(Formatter.Annotation).Count()}");

        //var workspace = new AdhocWorkspace();

        //var formattedRoot =
        //    Formatter.Format(
        //        newRoot,
        //        Formatter.Annotation,
        //        workspace);

        //Console.WriteLine("===== AFTER FORMAT =====");
        //Console.WriteLine(formattedRoot.ToFullString());
        //Console.WriteLine("========================");

        //var newText =
        //    formattedRoot.ToFullString();

        var workspace =
            new AdhocWorkspace();

        var projectId =
            ProjectId.CreateNewId();

        var documentId =
            DocumentId.CreateNewId(projectId);

        var projectInfo =
            ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Sentinel.Diagnostics.Instrumentation",
                "Sentinel.Diagnostics.Instrumentation",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                metadataReferences: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                }
            );

        workspace.AddProject(projectInfo);

        //workspace.AddDocument(
        //    projectId,
        //    Path.GetFileName(sourcePath),
        //    SourceText.From(newRoot.ToFullString()));
        workspace.AddDocument(
            DocumentInfo.Create(
                documentId,
                Path.GetFileName(sourcePath),
                loader: TextLoader.From(
                    TextAndVersion.Create(
                        SourceText.From(newRoot.ToFullString()),
                        VersionStamp.Create()))
            )
        );

        // REQUIRED: commit changes so Roslyn builds syntax trees
        workspace.TryApplyChanges(workspace.CurrentSolution);

        var document =
            workspace.CurrentSolution
                .GetDocument(documentId)!;

        document ??=
                workspace.CurrentSolution.Projects
                    .First(p => p.Id == projectId)
                    .Documents
                    .First();

        var formattedDocument =
            await Formatter.FormatAsync(
                document,
                Formatter.Annotation);

        var formattedRoot =
            await formattedDocument.GetSyntaxRootAsync();

        Console.WriteLine("===== AFTER FORMAT =====");
        Console.WriteLine(formattedRoot!.ToFullString());
        Console.WriteLine("========================");

        var newText =
            formattedRoot!.ToFullString();

        File.Copy(
            sourcePath,
            sourcePath + ".bak",
            overwrite: true);

        await File.WriteAllTextAsync(
            sourcePath,
            newText);
    }

    private static async Task RunInspectionAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: sentinel sentinel-cli <path-to-sln>");
            return;
        }
        
        string solutionPath = args[1];

        Console.WriteLine();
        Console.WriteLine("=== Sentinel Diagnostics CLI: Inspection Mode ===");
        Console.WriteLine($"Solution: {solutionPath}");
        Console.WriteLine();

        var workspace = new Sentinel.Diagnostics.Cli.Workspace.SentinelWorkspace();
        var solution = await workspace.LoadSolutionAsync(solutionPath);

        if (solution == null)
        {
            Console.WriteLine("ERROR: Solution could not be loaded.");
            return;
        }

        //var inspector = new Sentinel.Diagnostics.Cli.Workspace.SolutionInspector();
        //await inspector.ScanAsync(solution);
        await SentinelWorkspace.ScanAsync(solution);

        Console.WriteLine();
        Console.WriteLine("=== Inspection Complete ===");
        Console.WriteLine();
    }
}

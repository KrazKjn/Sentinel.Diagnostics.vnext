using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Sentinel.Diagnostics.Cli.Workspace;

public sealed class SentinelWorkspaceV1
{
    private MSBuildWorkspace? _workspace;

    public async Task<Solution?> LoadSolutionAsync(string solutionPath)
    {
        RegisterMSBuild("8.0.*");

        //_workspace = MSBuildWorkspace.Create();
        _workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
        {
            ["MSBuildLoadSettings"] = "All",
            ["ProvideCommandLineArgs"] = "true"
        });

        RegisterDiagnostics(_workspace);

        Console.WriteLine($"Opening solution: {solutionPath}");
        var solution = await _workspace.OpenSolutionAsync(solutionPath);

        Console.WriteLine($"Loaded solution: {solution.FilePath}");
        Console.WriteLine($"Projects found: {solution.Projects.Count()}");

        LogProjectSummary(solution);

        return solution;
    }

    // ----------------------------------------------------------------------
    // MSBUILD REGISTRATION
    // ----------------------------------------------------------------------

    private void RegisterMSBuild2()
    {
        // IMPORTANT: Must be first executable call in Main before any Roslyn/MSBuild types are touched.
        // You already ensured this in Program.cs, but this method keeps the logic centralized.
        if (MSBuildLocator.IsRegistered)
            return;

        var instances = MSBuildLocator.QueryVisualStudioInstances().ToList();

        if (instances.Count > 0)
        {
            var instance = instances.First();
            Console.WriteLine($"Using MSBuild from Visual Studio: {instance.MSBuildPath}");
            MSBuildLocator.RegisterInstance(instance);
            return;
        }
        // Fallback to explicit SDK path
        const string sdkPath = @"C:\Program Files\dotnet\sdk\8.0.424";
        Console.WriteLine($"Using explicit SDK MSBuild: {sdkPath}");
        MSBuildLocator.RegisterMSBuildPath(sdkPath);
    }

    private void RegisterMSBuild(string? versionPattern = null)
    {
        if (MSBuildLocator.IsRegistered)
            return;

        // 1. Prefer .NET SDK MSBuild (most reliable for CLI tools)
        var dotnetSdkPath = GetDotnetSdkMsbuildPath(versionPattern);
        if (dotnetSdkPath != null)
        {
            Console.WriteLine($"Using .NET SDK MSBuild: {dotnetSdkPath}");
            MSBuildLocator.RegisterMSBuildPath(dotnetSdkPath);
            return;
        }

        // 2. Fallback: Visual Studio MSBuild
        var vsInstances = MSBuildLocator.QueryVisualStudioInstances().ToList();
        if (vsInstances.Count > 0)
        {
            var instance = vsInstances.First();
            Console.WriteLine($"Using Visual Studio MSBuild: {instance.MSBuildPath}");
            MSBuildLocator.RegisterInstance(instance);
            return;
        }

        throw new InvalidOperationException(
            "No MSBuild instance found. Install .NET SDK or Visual Studio.");
    }

    private string? GetDotnetSdkMsbuildPath(string? versionPattern = null)
    {
        var sdkRoot = @"C:\Program Files\dotnet\sdk";

        if (!Directory.Exists(sdkRoot))
            return null;

        // Only include directories that look like SDK versions
        var sdkDirs = Directory.GetDirectories(sdkRoot)
            .Select(Path.GetFileName)
            .Where(IsValidSdkVersionDirectory)
            .ToList();

        if (sdkDirs.Count == 0)
            return null;

        // No pattern → newest SDK
        if (string.IsNullOrWhiteSpace(versionPattern))
            return Path.Combine(sdkRoot, sdkDirs.OrderByDescending(v => v).First());

        // Exact match
        if (!versionPattern.Contains('*'))
        {
            var exact = sdkDirs.FirstOrDefault(v =>
                v.Equals(versionPattern, StringComparison.OrdinalIgnoreCase));

            return exact != null ? Path.Combine(sdkRoot, exact) : null;
        }

        // Wildcard match
        var prefix = versionPattern.TrimEnd('*');

        var matches = sdkDirs
            .Where(v => v.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v)
            .ToList();

        if (matches.Count == 0)
            return null;

        return Path.Combine(sdkRoot, matches.First());
    }

    private bool IsValidSdkVersionDirectory(string? dirName)
    {
        if (string.IsNullOrWhiteSpace(dirName))
            return false;

        // Must start with a digit
        if (!char.IsDigit(dirName[0]))
            return false;

        // Must contain at least one dot
        if (!dirName.Contains('.'))
            return false;

        // Must parse as a version
        return Version.TryParse(dirName, out _);
    }


    // ----------------------------------------------------------------------
    // WORKSPACE DIAGNOSTICS
    // ----------------------------------------------------------------------

    private void RegisterDiagnostics(MSBuildWorkspace workspace)
    {
        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            Console.WriteLine($"WORKSPACE ERROR: {args.Diagnostic.Kind}: {args.Diagnostic.Message}");
        });
    }

    // ----------------------------------------------------------------------
    // PROJECT SUMMARY LOGGING
    // ----------------------------------------------------------------------

    private void LogProjectSummary(Solution solution)
    {
        Console.WriteLine();
        Console.WriteLine("=== Project Summary ===");

        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Project: {project.Name}");
            Console.WriteLine($"  File: {project.FilePath}");
            Console.WriteLine($"  Language: {project.Language}");
            Console.WriteLine($"  Documents: {project.Documents.Count()}");

            var tfm = project.ParseOptions?.PreprocessorSymbolNames
                .FirstOrDefault(s => s.StartsWith("NET"));
            if (string.IsNullOrWhiteSpace(tfm))
                tfm = TargetFrameworkResolver.ResolveFromOutputPath(project.OutputFilePath);

            Console.WriteLine($"  Target Framework: {tfm ?? "Unknown"}");

            Console.WriteLine();
        }
    }

    // ----------------------------------------------------------------------
    // FILE CLASSIFICATION HELPERS
    // ----------------------------------------------------------------------

    public static bool IsGenerated(Document doc)
    {
        var path = doc.FilePath ?? string.Empty;

        return path.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("GlobalUsings.g.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("ImplicitUsings.g.cs", StringComparison.OrdinalIgnoreCase);
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

    public static string GetDocumentKind(Document doc)
    {
        return doc.SourceCodeKind switch
        {
            SourceCodeKind.Regular => "Code",
            SourceCodeKind.Script => "Script",
            _ => "Other"
        };
    }
}

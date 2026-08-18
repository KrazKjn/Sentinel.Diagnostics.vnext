using Sentinel.Diagnostics.Cli.Workspace;

namespace Sentinel.Diagnostics.Cli.Commands;

public sealed class InspectCommand
{
    private readonly SentinelWorkspace _workspace;
    private readonly SolutionInspector _inspector;

    public InspectCommand()
    {
        _workspace = new SentinelWorkspace();
        _inspector = new SolutionInspector();
    }

    public async Task ExecuteAsync(string solutionPath)
    {
        Console.WriteLine();
        Console.WriteLine("=== Sentinel Diagnostics: Solution Inspection ===");
        Console.WriteLine($"Loading solution: {solutionPath}");
        Console.WriteLine();

        var solution = await _workspace.LoadSolutionAsync(solutionPath);

        if (solution == null)
        {
            Console.WriteLine("ERROR: Solution could not be loaded.");
            return;
        }

        Console.WriteLine("Solution loaded successfully.");
        Console.WriteLine();

        await _inspector.ScanAsync(solution);

        Console.WriteLine();
        Console.WriteLine("=== Inspection Complete ===");
        Console.WriteLine();
    }
}

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Sentinel.Diagnostics.Generator.Tests.Validation;

/// <summary>
/// A lightweight test double for SourceProductionContext.
/// Allows MetadataValidator to report diagnostics during tests.
/// </summary>
internal sealed class FakeSourceProductionContext
{
    public List<Diagnostic> Diagnostics { get; } = new();

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
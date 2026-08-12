namespace Sentinel.Diagnostics.Generator.Configuration;


/// <summary>
/// Fully resolved Sentinel Diagnostics configuration for a method.
///
/// Unlike AutoLogOptions, no values are nullable. All configuration
/// decisions have been resolved through the project, containing-type,
/// and method hierarchy.
/// </summary>
public sealed class EffectiveAutoLogOptions
{
    public bool Enabled { get; init; }
    public bool AddUsing { get; init; }
    public bool AddTryCatch { get; init; }
    public bool LogParameters { get; init; }
    public bool LogDuration { get; init; }
    public string Policy { get; init; } = "";
    public string Span { get; init; } = "";
}

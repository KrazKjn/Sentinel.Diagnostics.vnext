using System;
namespace Sentinel.Diagnostics.Core.Attributes;

/// <summary>
/// Marks a method for automatic Sentinel Diagnostics instrumentation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AutoLogAttribute : Attribute
{
    public AutoLogAttribute(string? Policy = null, string? Span = null)
    {
        this.Policy = Policy;
        this.Span = Span;
    }

    /// <summary>
    /// Gets or sets the Sentinel Diagnostics policy name.
    /// </summary>
    public string? Policy { get; }

    /// <summary>
    /// Gets or sets the OpenTelemetry span name.
    /// When not specified, the method name is used.
    /// </summary>
    public string? Span { get; }

    public bool? Enabled { get; set; }

    public bool? AddUsing { get; set; }

    public bool? AddTryCatch { get; set; }

    public bool? LogParameters { get; set; }

    public bool? LogDuration { get; set; }
}

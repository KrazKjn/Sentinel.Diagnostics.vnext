using System;
namespace Sentinel.Diagnostics.Core.Attributes;

/// <summary>
/// Marks a method for automatic Sentinel Diagnostics instrumentation.
/// </summary>
[AttributeUsage(
    AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = false)]
public sealed class AutoLogAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the Sentinel Diagnostics policy name.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Gets or sets the OpenTelemetry span name.
    /// When not specified, the method name is used.
    /// </summary>
    public string? Span { get; set; }
}
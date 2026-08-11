using System;

namespace Sentinel.Diagnostics.Core.Metadata;

/// <summary>
/// Describes a method parameter for Sentinel Diagnostics.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="SentinelParameterMetadata"/> class.
/// </remarks>
/// <param name="name">The parameter name.</param>
/// <param name="parameterType">The parameter's runtime type.</param>
/// <param name="isSensitive">
/// Indicates whether the parameter contains sensitive information.
/// </param>
/// <param name="shouldLog">
/// Indicates whether the parameter value should be logged.
/// </param>
public sealed class SentinelParameterMetadata(
    string name,
    Type parameterType,
    bool isSensitive = false,
    bool shouldLog = true)
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the parameter's runtime type.
    /// </summary>
    public Type ParameterType { get; } = parameterType;

    /// <summary>
    /// Gets a value indicating whether the parameter contains
    /// sensitive information.
    /// </summary>
    public bool IsSensitive { get; } = isSensitive;

    /// <summary>
    /// Gets a value indicating whether the parameter value should
    /// be included in diagnostic logging.
    /// </summary>
    public bool ShouldLog { get; } = shouldLog;
}
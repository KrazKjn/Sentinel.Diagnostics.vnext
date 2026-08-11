using System;
using System.Collections.Generic;

namespace Sentinel.Diagnostics.Core.Metadata;

/// <summary>
/// Describes the diagnostic metadata associated with an instrumented method.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="SentinelMethodMetadata"/> class.
/// </remarks>
/// <param name="name">
/// The method name.
/// </param>
/// <param name="fullName">
/// The fully qualified method name.
/// </param>
/// <param name="returnType">
/// The method's return type.
/// </param>
/// <param name="isAsync">
/// Indicates whether the method is asynchronous.
/// </param>
/// <param name="hasCancellationToken">
/// Indicates whether the method accepts a CancellationToken.
/// </param>
/// <param name="parameters">
/// The method parameter metadata.
/// </param>
/// <param name="policy">
/// The diagnostic policy associated with the method.
/// </param>
/// <param name="span">
/// The OpenTelemetry span metadata associated with the method.
/// </param>
public sealed class SentinelMethodMetadata(
    string name,
    string fullName,
    Type returnType,
    bool isAsync,
    bool hasCancellationToken,
    IReadOnlyList<SentinelParameterMetadata> parameters,
    SentinelPolicyMetadata? policy,
    SentinelSpanMetadata span)
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the fully qualified method name.
    /// </summary>
    public string FullName { get; } = fullName;

    /// <summary>
    /// Gets the return type of the method.
    /// </summary>
    public Type ReturnType { get; } = returnType;

    /// <summary>
    /// Gets a value indicating whether the method is asynchronous.
    /// </summary>
    public bool IsAsync { get; } = isAsync;

    /// <summary>
    /// Gets a value indicating whether the method accepts
    /// a CancellationToken parameter.
    /// </summary>
    public bool HasCancellationToken { get; } = hasCancellationToken;

    /// <summary>
    /// Gets the metadata describing the method parameters.
    /// </summary>
    public IReadOnlyList<SentinelParameterMetadata> Parameters { get; } = parameters;

    /// <summary>
    /// Gets the diagnostic policy associated with the method.
    /// </summary>
    public SentinelPolicyMetadata? Policy { get; } = policy;

    /// <summary>
    /// Gets the OpenTelemetry span metadata associated with the method.
    /// </summary>
    public SentinelSpanMetadata Span { get; } = span;
}
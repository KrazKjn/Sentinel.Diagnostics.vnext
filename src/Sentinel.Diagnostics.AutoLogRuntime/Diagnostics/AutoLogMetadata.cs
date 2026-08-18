using System;
using System.Collections.Generic;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

/// <summary>
/// Compile-time generated metadata describing an AutoLog invocation.
/// </summary>
public sealed class AutoLogMetadata
{
    public AutoLogMetadata(
        string methodName,
        string fullName,
        IReadOnlyList<AutoLogParameter> parameters,
        int depth,
        Guid instanceId,
        string callPath)
    {
        MethodName = methodName;
        FullName = fullName;
        Parameters = parameters;
        Depth = depth;
        InstanceId = instanceId;
        CallPath = callPath;
    }

    public string MethodName { get; }

    public string FullName { get; }

    public IReadOnlyList<AutoLogParameter> Parameters { get; }

    public int Depth { get; }

    public Guid InstanceId { get; }

    public string CallPath { get; }
}
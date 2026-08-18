using System;

namespace Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

/// <summary>
/// Describes one parameter captured by generated instrumentation.
/// </summary>
public sealed class AutoLogParameter
{
    public AutoLogParameter(
        string name,
        Type type,
        object? value,
        bool isSensitive = false,
        bool shouldLog = true)
    {
        Name = name;
        Type = type;
        Value = value;
        IsSensitive = isSensitive;
        ShouldLog = shouldLog;
    }

    public string Name { get; }

    public Type Type { get; }

    public object? Value { get; }

    public bool IsSensitive { get; }

    public bool ShouldLog { get; }
}
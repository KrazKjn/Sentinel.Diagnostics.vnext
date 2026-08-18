using Sentinel.Diagnostics.Generator.Configuration;
using System;
using System.Collections.Generic;

public sealed class ProjectAutoLogOptionsComparer : IEqualityComparer<ProjectAutoLogOptions>
{
    public static readonly ProjectAutoLogOptionsComparer Instance = new();

    public bool Equals(ProjectAutoLogOptions? x, ProjectAutoLogOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return x.Enabled == y.Enabled &&
               x.AddUsing == y.AddUsing &&
               x.AddTryCatch == y.AddTryCatch &&
               x.LogParameters == y.LogParameters &&
               x.LogDuration == y.LogDuration &&
               x.Policy == y.Policy &&
               x.Span == y.Span;
    }

    public int GetHashCode(ProjectAutoLogOptions obj)
    {
        return HashCode.Combine(
            obj.Enabled,
            obj.AddUsing,
            obj.AddTryCatch,
            obj.LogParameters,
            obj.LogDuration,
            obj.Policy,
            obj.Span);
    }
}

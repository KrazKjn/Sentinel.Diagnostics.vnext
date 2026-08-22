using System;
using System.Threading;

namespace Sentinel.Diagnostics.Core.Runtime.Context;

public static class SentinelOperationContext
{
    private static readonly AsyncLocal<Guid> _id = new();

    public static Guid CurrentOperationId
    {
        get => _id.Value;
        set => _id.Value = value;
    }
}

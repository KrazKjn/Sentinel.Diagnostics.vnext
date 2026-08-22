using System;
using System.Threading;

namespace Sentinel.Diagnostics.Core.Runtime.Context;

public static class SentinelCorrelationContext
{
    private static readonly AsyncLocal<Guid> _id = new();

    public static Guid CorrelationId
    {
        get => _id.Value == Guid.Empty ? (_id.Value = Guid.NewGuid()) : _id.Value;
        set => _id.Value = value;
    }
}

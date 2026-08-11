using System.Collections.Generic;

namespace Sentinel.Diagnostics.Core.Metadata;

public sealed class SentinelSpanMetadata(
    string name,
    IReadOnlyDictionary<string, object?> attributes)
{
    public string Name { get; } = name;
    public IReadOnlyDictionary<string, object?> Attributes { get; } = attributes;
}

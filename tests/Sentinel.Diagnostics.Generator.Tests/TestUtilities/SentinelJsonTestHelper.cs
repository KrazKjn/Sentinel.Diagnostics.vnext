using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Sentinel.Diagnostics.Generator.Tests.TestHelpers;

public sealed class InMemoryAdditionalText : AdditionalText
{
    private readonly string _path;
    private readonly string _text;

    public InMemoryAdditionalText(string path, string text)
    {
        _path = path;
        _text = text;
    }

    public override string Path => _path;

    public override SourceText? GetText(CancellationToken cancellationToken = default)
        => SourceText.From(_text, Encoding.UTF8);
}

public static class SentinelJsonTestHelper
{
    public static AdditionalText CreateSentinelJson(string json)
        => new InMemoryAdditionalText("sentinel.json", json);
}

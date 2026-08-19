using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics.Constants;
using Sentinel.Diagnostics.AutoLogRuntime.Logging.Constants;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging.Internal;

public static class LogLineParser
{
    private static readonly Regex LogRegex = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z)\s+" +
        @"(?<guid>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}):\s*" +
        @"(?<text>.*)$",
        RegexOptions.Compiled);

    public static bool TryParse(string input, out DateTime timestamp, out Guid guid, out int indented, out string text)
    {
        timestamp = default;
        guid = default;
        indented = 0;
        text = string.Empty;

        var match = LogRegex.Match(input);
        if (!match.Success)
            return false;

        // Extract timestamp
        if (!DateTime.TryParse(match.Groups["timestamp"].Value, out timestamp))
            return false;

        // Extract GUID
        if (!Guid.TryParse(match.Groups["guid"].Value, out guid))
            return false;

        // Extract remaining text
        //text = match.Groups["text"].Value;
        var guidDisplay = guid.ToString();
        int pos = input.IndexOf(guidDisplay) + guidDisplay.Length + 1;
        text = input[pos..];

        ReadOnlySpan<char> span = text.AsSpan();

        int index = -1;

        for (int i = 0; i < span.Length; i++)
        {
            if (!char.IsWhiteSpace(span[i]))
            {
                index = i;
                break;
            }
        }
        if (index > 0)
        {
            indented = index / LoggingConstants.IndentationMultiplier;
        }

        return true;
    }
}

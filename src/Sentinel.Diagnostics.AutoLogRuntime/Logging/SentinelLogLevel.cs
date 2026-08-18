using System.Text.Json.Serialization;

namespace Sentinel.Diagnostics.AutoLogRuntime.Logging;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SentinelLogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
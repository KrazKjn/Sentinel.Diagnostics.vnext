namespace Sentinel.Diagnostics.Generator.Configuration
{
    public sealed class ProjectAutoLogOptions
    {
        public bool? Enabled { get; init; }
        public bool? AddUsing { get; init; }
        public bool? AddTryCatch { get; init; }
        public bool? LogParameters { get; init; }
        public bool? LogDuration { get; init; }
        public string? Policy { get; init; }
        public string? Span { get; init; }
    }
}

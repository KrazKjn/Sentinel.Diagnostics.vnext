using System;
using System.Collections.Generic;
using System.Text;

namespace Sentinel.Diagnostics.Generator.Metadata
{
    internal sealed record AutoLogOptionsOverride(
        bool? Enabled = null,
        bool? AddUsing = null,
        bool? AddTryCatch = null,
        bool? LogParameters = null,
        bool? LogDuration = null,
        string? Policy = null,
        string? Span = null);
}

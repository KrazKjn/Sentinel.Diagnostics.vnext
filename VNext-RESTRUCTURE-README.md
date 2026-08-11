# Sentinel Diagnostics vNext

This repository is the working reconstruction of Sentinel Diagnostics
following the pre-instrumentation baseline.

The baseline repository is preserved separately.

## Architectural principles

1. Sentinel is unobtrusive.
2. [AutoLog] remains the instrumentation directive.
3. Core is independent of third-party logging frameworks.
4. The generator performs semantic analysis and metadata generation.
5. The instrumentation engine modifies source.
6. Runtime diagnostics preserve the original exception behavior.
7. Sensitive parameters are never logged.
8. Project configuration can be overridden at class and method level.

## Major pipeline

Source
    â†“
Instrumentation Configuration
    â†“
Roslyn Instrumentation
    â†“
C# Compilation
    â†“
Incremental Generator
    â†“
Generated Metadata
    â†“
Sentinel Diagnostics Runtime

## Third-party logging

Sentinel.Diagnostics.Core must not reference:

- Serilog
- log4net
- NLog
- Microsoft.Extensions.Logging

Adapters may be created separately.

## Important

This repository is intentionally not considered complete until:

- the solution builds
- the generator builds
- instrumentation tests pass
- the sample executes
- exception propagation is verified
- sensitive-data filtering is verified
- logger independence is verified

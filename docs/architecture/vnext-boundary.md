# Sentinel Diagnostics vNext Architectural Boundary

## Core

Owns diagnostic runtime behavior and abstractions.

Core must not depend on third-party logging frameworks.

## Generator

Performs compile-time syntax and semantic analysis and produces
diagnostic metadata.

The generator does not rewrite existing method bodies.

## Instrumentation

Performs source transformation using Roslyn.

Instrumentation applies the effective AutoLog configuration to
existing source methods.

## CLI

Provides the developer-facing instrumentation engine.

## Logging

Sentinel produces diagnostic events through Sentinel abstractions.

Third-party logging frameworks are integration adapters and are
not dependencies of Sentinel.Diagnostics.Core.

## Runtime

The diagnostic execution envelope captures:

- Method entry
- Parameters
- Method exit
- Duration
- Exceptions
- Span information
- Policy information
- Nested execution context

Exceptions are observed and rethrown without altering application
behavior.

## Configuration

Configuration hierarchy:

Project
    â†“
Class
    â†“
Method

The most specific configuration wins.

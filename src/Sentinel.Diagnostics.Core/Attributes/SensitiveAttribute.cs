using System;

namespace Sentinel.Diagnostics.Core.Attributes;

/// <summary>
/// Marks a method parameter as containing sensitive information
/// that must not be included in diagnostic logging.
/// </summary>
[AttributeUsage(
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = false)]
public sealed class SensitiveAttribute : Attribute
{
}
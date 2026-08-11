using System;

namespace Sentinel.Diagnostics.Generator.Compatibility;

/// <summary>
/// Provides argument validation helpers for the generator.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> when
    /// <paramref name="value"/> is null.
    /// </summary>
    /// <typeparam name="T">
    /// The argument type.
    /// </typeparam>
    /// <param name="value">
    /// The value to validate.
    /// </param>
    /// <param name="parameterName">
    /// The name of the argument.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null.
    /// </exception>
    public static void NotNull<T>(
        T? value,
        string parameterName)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
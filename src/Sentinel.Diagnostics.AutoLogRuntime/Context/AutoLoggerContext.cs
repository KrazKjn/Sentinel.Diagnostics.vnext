using System.Threading;

namespace Sentinel.Diagnostics.AutoLogRuntime.Context;

/// <summary>
/// Maintains diagnostic call depth for the current asynchronous execution flow.
/// </summary>
public static class AutoLoggerContext
{
    private static readonly AsyncLocal<DepthState?> CurrentState = new();

    /// <summary>
    /// Gets the current diagnostic depth.
    /// </summary>
    public static int CurrentDepth =>
        CurrentState.Value?.Depth ?? 0;

    /// <summary>
    /// Increments the diagnostic depth.
    /// </summary>
    public static int IncrementDepth()
    {
        var state = CurrentState.Value;

        if (state is null)
        {
            state = new DepthState();
            CurrentState.Value = state;
        }

        state.Depth++;

        return state.Depth;
    }

    /// <summary>
    /// Decrements the diagnostic depth.
    /// </summary>
    public static void DecrementDepth()
    {
        var state = CurrentState.Value;

        if (state is null)
        {
            return;
        }

        if (state.Depth > 0)
        {
            state.Depth--;
        }

        if (state.Depth == 0)
        {
            CurrentState.Value = null;
        }
    }

    private sealed class DepthState
    {
        public int Depth;
    }
}
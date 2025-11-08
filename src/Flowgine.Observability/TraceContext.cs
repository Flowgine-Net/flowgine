namespace Flowgine.Observability;

/// <summary>
/// Provides ambient trace context using AsyncLocal storage.
/// This allows trace context to flow through async operations without explicit passing.
/// </summary>
public static class TraceContext
{
    private static readonly AsyncLocal<ITraceContext?> _current = new();

    /// <summary>
    /// Gets or sets the current trace context for the async flow.
    /// </summary>
    public static ITraceContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}


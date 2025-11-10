namespace Flowgine.Observability;

/// <summary>
/// Provides ambient trace context using AsyncLocal storage.
/// This allows trace context to flow through async operations without explicit passing.
/// </summary>
public static class TraceContext
{
    private static readonly AsyncLocal<ITraceContext?> _current = new();
    private static ITraceContext? _globalCurrent;
    private static readonly object _globalLock = new();

    /// <summary>
    /// Gets or sets the current trace context for the async flow.
    /// </summary>
    public static ITraceContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    /// <summary>
    /// Gets or sets the global fallback trace context when AsyncLocal is lost (e.g., across async boundaries).
    /// This property is thread-safe.
    /// </summary>
    public static ITraceContext? GlobalCurrent 
    { 
        get
        {
            lock (_globalLock)
            {
                return _globalCurrent;
            }
        }
        set
        {
            lock (_globalLock)
            {
                _globalCurrent = value;
            }
        }
    }
}


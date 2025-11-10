namespace Flowgine.Observability;

/// <summary>
/// Represents a trace context for tracking the entire execution of a flow.
/// A trace can contain multiple spans representing individual operations.
/// </summary>
public interface ITraceContext
{
    /// <summary>
    /// Gets the unique identifier for this trace.
    /// </summary>
    string TraceId { get; }
    
    /// <summary>
    /// Gets the metadata dictionary for storing custom trace-level information.
    /// </summary>
    Dictionary<string, object> Metadata { get; }
}

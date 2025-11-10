namespace Flowgine.Observability;

/// <summary>
/// Represents a span context for tracking the execution of a single operation or node.
/// </summary>
public interface ISpanContext
{
    /// <summary>
    /// Gets the unique identifier for this span.
    /// </summary>
    string SpanId { get; }
    
    /// <summary>
    /// Gets the UTC timestamp when this span started.
    /// </summary>
    DateTime StartTime { get; }
}

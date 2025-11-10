using System.Diagnostics;

namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Langfuse-specific implementation of <see cref="ISpanContext"/> backed by OpenTelemetry Activity.
/// Tracks individual node operations within a flow.
/// </summary>
public sealed class LangfuseSpanContext : ISpanContext
{
    /// <summary>
    /// Gets the underlying OpenTelemetry Activity for this span.
    /// </summary>
    public Activity? Activity { get; } 
    
    /// <inheritdoc />
    public string SpanId => Activity?.SpanId.ToString() ?? ""; 
    
    /// <inheritdoc />
    public DateTime StartTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LangfuseSpanContext"/> class.
    /// </summary>
    /// <param name="activity">The OpenTelemetry activity representing this span.</param>
    public LangfuseSpanContext(Activity? activity)
    {
        Activity = activity; 
        StartTime = DateTime.UtcNow;
    }
}
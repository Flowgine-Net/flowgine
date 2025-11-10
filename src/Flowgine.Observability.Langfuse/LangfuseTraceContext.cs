using System.Diagnostics;

namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Langfuse-specific implementation of <see cref="ITraceContext"/> backed by OpenTelemetry Activity.
/// Represents a complete flow execution tracked in Langfuse.
/// </summary>
public class LangfuseTraceContext : ITraceContext
{
    /// <summary>
    /// Gets the underlying OpenTelemetry Activity for this trace.
    /// </summary>
    public Activity? Activity { get; } 
    
    /// <inheritdoc />
    public string TraceId => Activity?.TraceId.ToString() ?? ""; 
    
    /// <inheritdoc />
    public Dictionary<string, object> Metadata { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LangfuseTraceContext"/> class.
    /// </summary>
    /// <param name="activity">The OpenTelemetry activity representing this trace.</param>
    /// <param name="runId">The unique identifier for this flow execution run.</param>
    public LangfuseTraceContext(Activity? activity, Guid runId)
    {
        Activity = activity; 
        Metadata["run_id"] = runId;
    }
}
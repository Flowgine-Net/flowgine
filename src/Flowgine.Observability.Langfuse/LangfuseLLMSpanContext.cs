using System.Diagnostics;

namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Langfuse-specific implementation of <see cref="ILLMSpanContext"/> backed by OpenTelemetry Activity.
/// Tracks LLM generation operations for the Langfuse platform.
/// </summary>
public class LangfuseLLMSpanContext : ILLMSpanContext
{
    /// <summary>
    /// Gets the underlying OpenTelemetry Activity for this LLM span.
    /// </summary>
    public Activity? Activity { get; } 
    
    /// <inheritdoc />
    public string SpanId => Activity?.SpanId.ToString() ?? ""; 
    
    /// <inheritdoc />
    public DateTime StartTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LangfuseLLMSpanContext"/> class.
    /// </summary>
    /// <param name="activity">The OpenTelemetry activity representing this LLM span.</param>
    public LangfuseLLMSpanContext(Activity? activity)
    {
        Activity = activity; 
        StartTime = DateTime.UtcNow;
    }
}
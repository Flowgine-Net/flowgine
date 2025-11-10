using Flowgine.LLM.Abstractions;

namespace Flowgine.Observability;

/// <summary>
/// Defines the contract for observability providers that track flow execution and LLM interactions.
/// Providers like Langfuse, OpenTelemetry, or custom implementations can implement this interface.
/// </summary>
public interface IObservabilityProvider
{
    /// <summary>
    /// Starts a new trace for a flow execution.
    /// </summary>
    /// <param name="name">The name of the flow being executed.</param>
    /// <param name="runId">Unique identifier for this execution run.</param>
    /// <param name="input">Optional input data to log at the start of the trace.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A trace context for tracking this execution.</returns>
    Task<ITraceContext> StartTraceAsync(string name, Guid runId, string? input = null, CancellationToken ct = default);
    
    /// <summary>
    /// Ends a trace and flushes any pending telemetry.
    /// </summary>
    /// <param name="trace">The trace context to end.</param>
    /// <param name="output">Optional output data to log at the end of the trace.</param>
    /// <param name="ct">Cancellation token.</param>
    Task EndTraceAsync(ITraceContext trace, string? output = null, CancellationToken ct = default);

    /// <summary>
    /// Starts a new span for a node execution within a trace.
    /// </summary>
    /// <param name="trace">The parent trace context.</param>
    /// <param name="nodeName">The name of the node being executed.</param>
    /// <param name="input">The input state for the node.</param>
    /// <param name="observationType">Optional observation type (e.g., "agent", "tool", "chain").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A span context for tracking this node execution.</returns>
    Task<ISpanContext> StartSpanAsync(ITraceContext trace, string nodeName, object? input, string? observationType = null, CancellationToken ct = default);
    
    /// <summary>
    /// Ends a span and records the output or error.
    /// </summary>
    /// <param name="span">The span context to end.</param>
    /// <param name="output">The output state from the node.</param>
    /// <param name="error">Optional exception if the node failed.</param>
    /// <param name="ct">Cancellation token.</param>
    Task EndSpanAsync(ISpanContext span, object? output, Exception? error = null, CancellationToken ct = default);

    /// <summary>
    /// Starts a new span specifically for LLM API calls.
    /// </summary>
    /// <param name="trace">The parent trace context.</param>
    /// <param name="modelName">The name/identifier of the LLM model being called.</param>
    /// <param name="request">The chat request being sent to the LLM.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An LLM span context for tracking this API call.</returns>
    Task<ILLMSpanContext> StartLLMSpanAsync(ITraceContext trace, string modelName, ChatRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Ends an LLM span and records the completion or error.
    /// </summary>
    /// <param name="span">The LLM span context to end.</param>
    /// <param name="completion">The completion response from the LLM, if successful.</param>
    /// <param name="error">Optional exception if the LLM call failed.</param>
    /// <param name="ct">Cancellation token.</param>
    Task EndLLMSpanAsync(ILLMSpanContext span, ChatCompletion? completion, Exception? error = null, CancellationToken ct = default);
}

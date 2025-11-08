using Flowgine.LLM.Abstractions;

namespace Flowgine.Observability;

public interface IObservabilityProvider
{
    // Flow-level tracking
    Task<ITraceContext> StartTraceAsync(string name, Guid runId, CancellationToken ct = default);

    // Node-level tracking
    Task<ISpanContext> StartSpanAsync(ITraceContext trace, string nodeName, object? input, CancellationToken ct = default);
    Task EndSpanAsync(ISpanContext span, object? output, Exception? error = null, CancellationToken ct = default);

    // LLM-specific tracking
    Task<ILLMSpanContext> StartLLMSpanAsync(ITraceContext trace, string modelName, ChatRequest request, CancellationToken ct = default);
    Task EndLLMSpanAsync(ILLMSpanContext span, ChatCompletion? completion, Exception? error = null, CancellationToken ct = default);
}

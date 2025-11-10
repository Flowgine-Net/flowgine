using Flowgine.LLM.Abstractions;
using System.Runtime.CompilerServices;

namespace Flowgine.Observability;

/// <summary>
/// Decorator for <see cref="IChatModel"/> that adds observability tracking to all LLM interactions.
/// Automatically captures requests, responses, token usage, and errors.
/// </summary>
public sealed class ObservableChatModel : IChatModel
{
    private readonly IChatModel _inner;
    private readonly IObservabilityProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableChatModel"/> class.
    /// </summary>
    /// <param name="inner">The underlying chat model to wrap.</param>
    /// <param name="provider">The observability provider for tracking calls.</param>
    public ObservableChatModel(
        IChatModel inner,
        IObservabilityProvider provider)
    {
        _inner = inner;
        _provider = provider;
    }

    /// <inheritdoc />
    public string Model => _inner.Model;

    /// <inheritdoc />
    public async Task<ChatCompletion> GenerateAsync(
        ChatRequest request,
        CancellationToken ct = default)
    {
        // Try to get trace context from AsyncLocal first, then from global static fallback
        var trace = TraceContext.Current ?? TraceContext.GlobalCurrent;
        
        if (trace == null)
            return await _inner.GenerateAsync(request, ct);

        var span = await _provider.StartLLMSpanAsync(trace, _inner.Model, request, ct);

        try
        {
            var result = await _inner.GenerateAsync(request, ct);
            await _provider.EndLLMSpanAsync(span, result, null, ct);
            return result;
        }
        catch (Exception ex)
        {
            await _provider.EndLLMSpanAsync(span, null, ex, ct);
            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var trace = TraceContext.Current ?? TraceContext.GlobalCurrent;
        if (trace == null)
        {
            await foreach (var ev in _inner.StreamAsync(request, ct))
                yield return ev;
            yield break;
        }

        var span = await _provider.StartLLMSpanAsync(trace, _inner.Model, request, ct);
        ChatCompletion? completion = null;
        Exception? error = null;

        IAsyncEnumerator<ChatStreamEvent>? enumerator = null;
        try
        {
            enumerator = _inner.StreamAsync(request, ct).GetAsyncEnumerator(ct);
            
            while (true)
            {
                ChatStreamEvent ev;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                        break;
                    ev = enumerator.Current;
                }
                catch (Exception ex)
                {
                    error = ex;
                    throw;
                }

                if (ev is Completed c)
                    completion = c.Completion;

                yield return ev;
            }
        }
        finally
        {
            if (enumerator != null)
                await enumerator.DisposeAsync();
                
            await _provider.EndLLMSpanAsync(span, completion, error, ct);
        }
    }
}

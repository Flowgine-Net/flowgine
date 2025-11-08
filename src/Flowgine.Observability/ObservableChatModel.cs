using Flowgine.LLM.Abstractions;
using System.Runtime.CompilerServices;

namespace Flowgine.Observability;

public sealed class ObservableChatModel : IChatModel
{
    private readonly IChatModel _inner;
    private readonly IObservabilityProvider _provider;
    private readonly string _modelName;

    public ObservableChatModel(
        IChatModel inner,
        IObservabilityProvider provider,
        string modelName)
    {
        _inner = inner;
        _provider = provider;
        _modelName = modelName;
    }

    public async Task<ChatCompletion> GenerateAsync(
        ChatRequest request,
        CancellationToken ct = default)
    {
        // Get current trace context from AsyncLocal or Runtime
        var trace = TraceContext.Current;
        if (trace == null)
            return await _inner.GenerateAsync(request, ct);

        var span = await _provider.StartLLMSpanAsync(trace, _modelName, request, ct);

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

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var trace = TraceContext.Current;
        if (trace == null)
        {
            await foreach (var ev in _inner.StreamAsync(request, ct))
                yield return ev;
            yield break;
        }

        var span = await _provider.StartLLMSpanAsync(trace, _modelName, request, ct);
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

using Flowgine.Abstractions;
using Flowgine.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Flowgine.Observability;

public sealed class ObservableCompiledFlowgine<TState>
{
    private readonly CompiledFlowgine<TState> _inner;
    private readonly IObservabilityProvider _provider;

    public ObservableCompiledFlowgine(
        CompiledFlowgine<TState> inner,
        IObservabilityProvider provider)
    {
        _inner = inner;
        _provider = provider;
    }

    public async IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState,
        IServiceProvider? services,
        Guid runId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var trace = await _provider.StartTraceAsync(_inner.Name, runId, ct);
        
        // Set the trace context for the current async flow
        var previousTrace = TraceContext.Current;
        TraceContext.Current = trace;
        
        ISpanContext? currentSpan = null;

        try
        {
            await foreach (var ev in _inner.RunAsync(initialState, services, runId, ct))
            {
                switch (ev)
                {
                    case NodeStarted<TState> started:
                        currentSpan = await _provider.StartSpanAsync(
                            trace, started.NodeName, initialState, ct);
                        break;

                    case NodeCompleted<TState> completed:
                        if (currentSpan != null)
                        {
                            await _provider.EndSpanAsync(
                                currentSpan, completed.State, null, ct);
                            currentSpan = null;
                        }
                        break;

                    case NodeFailed<TState> failed:
                        if (currentSpan != null)
                        {
                            await _provider.EndSpanAsync(
                                currentSpan, failed.State, failed.Error, ct);
                            currentSpan = null;
                        }
                        break;
                }

                yield return ev;
            }
        }
        finally
        {
            // Restore previous trace context
            TraceContext.Current = previousTrace;
            
            // Flush trace
            //await trace.EndAsync(ct);
        }
    }
}

using Flowgine.Abstractions;
using Flowgine.Core;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Flowgine.Observability;

/// <summary>
/// Decorator for <see cref="CompiledFlowgine{TState}"/> that adds observability tracking to flow execution.
/// Automatically captures node execution, state transitions, and errors.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public sealed class ObservableCompiledFlowgine<TState>
{
    private readonly CompiledFlowgine<TState> _inner;
    private readonly IObservabilityProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCompiledFlowgine{TState}"/> class.
    /// </summary>
    /// <param name="inner">The underlying compiled flow to wrap.</param>
    /// <param name="provider">The observability provider for tracking execution.</param>
    public ObservableCompiledFlowgine(
        CompiledFlowgine<TState> inner,
        IObservabilityProvider provider)
    {
        _inner = inner;
        _provider = provider;
    }

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public Task<TState> RunToCompletionAsync(
        TState initial,
        CancellationToken ct = default)
        => RunToCompletionAsync(initial, services: null, Guid.NewGuid(), ct);

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="services">Optional service provider for dependency injection.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public Task<TState> RunToCompletionAsync(
        TState initial,
        IServiceProvider? services,
        CancellationToken ct = default)
        => RunToCompletionAsync(initial, services, Guid.NewGuid(), ct);

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="services">Optional service provider for dependency injection.</param>
    /// <param name="runId">A unique identifier for this specific flow run, used for observability.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public async Task<TState> RunToCompletionAsync(
        TState initial,
        IServiceProvider? services,
        Guid runId,
        CancellationToken ct = default)
    {
        var final = initial;
        await foreach (var ev in RunAsync(initial, services, runId, ct))
            if (ev is NodeCompleted<TState> done)
                final = done.State;

        return final;
    }

    /// <summary>
    /// Asynchronously executes the compiled flow and yields events describing the execution progress.
    /// Tracks all execution steps using the configured observability provider.
    /// </summary>
    /// <param name="initialState">The initial state to start the flow execution with.</param>
    /// <param name="services">Optional service provider for dependency injection in nodes.</param>
    /// <param name="runId">A unique identifier for this specific flow run, used for tracing.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An async enumerable of flow events that describe the execution progress.</returns>
    public async IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState,
        IServiceProvider? services,
        Guid runId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Extract input for tracing
        var traceInput = ExtractTraceInput(initialState);
        var trace = await _provider.StartTraceAsync(_inner.Name, runId, traceInput, ct);
        
        // Set the trace context for the current async flow
        var previousTrace = TraceContext.Current;
        var previousGlobalTrace = TraceContext.GlobalCurrent;
        TraceContext.Current = trace;
        TraceContext.GlobalCurrent = trace; // Set global fallback in case AsyncLocal is lost
        
        ISpanContext? currentSpan = null;
        TState currentState = initialState; // Track the current state across nodes
        TState? finalState = default;

        try
        {
            await foreach (var ev in _inner.RunAsync(initialState, services, runId, ct))
            {
                switch (ev)
                {
                    case NodeStarted<TState> started:
                        // Extract metadata from node if it implements IObservableNode
                        string? observationType = started.Metadata?.GetValueOrDefault("observationType");

                        currentSpan = await _provider.StartSpanAsync(
                            trace, started.NodeName, currentState, observationType, ct); // Use current state, not initial
                        break;

                    case NodeCompleted<TState> completed:
                        if (currentSpan != null)
                        {
                            await _provider.EndSpanAsync(
                                currentSpan, completed.State, null, ct);
                            currentSpan = null;
                        }
                        currentState = completed.State; // Update state after node completion
                        finalState = completed.State;
                        break;

                    case NodeFailed<TState> failed:
                        if (currentSpan != null)
                        {
                            await _provider.EndSpanAsync(
                                currentSpan, failed.State, failed.Error, ct);
                            currentSpan = null;
                        }
                        currentState = failed.State; // Update state even on failure
                        finalState = failed.State;
                        break;
                }

                yield return ev;
            }
        }
        finally
        {
            // Restore previous trace context
            TraceContext.Current = previousTrace;
            TraceContext.GlobalCurrent = previousGlobalTrace;
            
            // Extract output for tracing
            var traceOutput = ExtractTraceOutput(finalState);
            
            // End trace to flush activities
            await _provider.EndTraceAsync(trace, traceOutput, ct);
        }
    }

    /// <summary>
    /// Extracts the trace input from the state.
    /// If the state implements <see cref="IObservableState"/>, uses its GetInput method.
    /// Otherwise, uses ToString().
    /// </summary>
    private string? ExtractTraceInput(TState state)
    {
        // If TState implements IObservableState, extract it
        if (state is IObservableState observableState)
            return observableState.GetInput();
        
        // Otherwise return the state itself as string
        return state?.ToString();
    }

    /// <summary>
    /// Extracts the trace output from the state.
    /// If the state implements <see cref="IObservableState"/>, uses its GetOutput method.
    /// Otherwise, uses ToString().
    /// </summary>
    private string? ExtractTraceOutput(TState? state)
    {
        // If TState implements IObservableState, extract it
        if (state is IObservableState observableState)
            return observableState.GetOutput();
        
        // Otherwise return the state itself as string
        return state?.ToString();
    }
}

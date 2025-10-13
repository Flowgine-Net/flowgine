namespace Flowgine.Abstractions;

/// <summary>
/// Represents a node that can perform asynchronous operations using a specified state.
/// </summary>
/// <typeparam name="TState">The type of state that is processed by the node.</typeparam>
public interface INode<TState>
{
    object? Invoke(TState state, Runtime runtime, CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously invokes the node's operation with the provided state and cancellation token.
    /// </summary>
    /// <param name="state">The state to be passed to the node during the invocation.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the invocation.</returns>
    // Async adaptér – výchozí implementace
    ValueTask<object?> InvokeAsync(TState state, Runtime runtime, CancellationToken ct = default)
        => new(Invoke(state, runtime, ct));
}

public abstract class AsyncNode<TState> : INode<TState>
{
    public object? Invoke(TState state, Runtime runtime, CancellationToken ct = default)
        => throw new NotSupportedException($"{GetType().Name} is async-only. Use InvokeAsync.");

    public abstract ValueTask<object?> InvokeAsync(
        TState state, Runtime runtime, CancellationToken ct = default);
}
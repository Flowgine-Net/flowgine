namespace Flowgine.Abstractions;

/// <summary>
/// Represents a node that can perform operations using a specified state.
/// Nodes are the building blocks of workflows in Flowgine.
/// </summary>
/// <typeparam name="TState">The type of state that is processed by the node.</typeparam>
public interface INode<TState>
{
    /// <summary>
    /// Synchronously invokes the node's operation with the provided state.
    /// </summary>
    /// <param name="state">The state to be passed to the node during the invocation.</param>
    /// <param name="runtime">The runtime context providing access to services and run information.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>The result of the node operation, which can be a command, partial state update, or null.</returns>
    object? Invoke(TState state, Runtime runtime, CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously invokes the node's operation with the provided state.
    /// Default implementation wraps the synchronous <see cref="Invoke"/> method.
    /// </summary>
    /// <param name="state">The state to be passed to the node during the invocation.</param>
    /// <param name="runtime">The runtime context providing access to services and run information.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the invocation.</returns>
    ValueTask<object?> InvokeAsync(TState state, Runtime runtime, CancellationToken ct = default)
        => new(Invoke(state, runtime, ct));
}

/// <summary>
/// Base class for nodes that only support asynchronous execution.
/// The synchronous <see cref="Invoke"/> method throws <see cref="NotSupportedException"/>.
/// </summary>
/// <typeparam name="TState">The type of state that is processed by the node.</typeparam>
public abstract class AsyncNode<TState> : INode<TState>
{
    /// <summary>
    /// Not supported for async-only nodes. Always throws <see cref="NotSupportedException"/>.
    /// Use <see cref="InvokeAsync"/> instead.
    /// </summary>
    /// <param name="state">The state (not used).</param>
    /// <param name="runtime">The runtime context (not used).</param>
    /// <param name="ct">The cancellation token (not used).</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? Invoke(TState state, Runtime runtime, CancellationToken ct = default)
        => throw new NotSupportedException($"{GetType().Name} is async-only. Use InvokeAsync.");

    /// <summary>
    /// Asynchronously invokes the node's operation with the provided state.
    /// </summary>
    /// <param name="state">The state to be passed to the node during the invocation.</param>
    /// <param name="runtime">The runtime context providing access to services and run information.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the invocation.</returns>
    public abstract ValueTask<object?> InvokeAsync(
        TState state, Runtime runtime, CancellationToken ct = default);
}
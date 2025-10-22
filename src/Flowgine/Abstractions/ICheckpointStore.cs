namespace Flowgine.Abstractions;

/// <summary>
/// Defines a contract for storing and retrieving flow execution checkpoints.
/// </summary>
/// <typeparam name="TState">The type of state to be checkpointed.</typeparam>
public interface ICheckpointStore<TState>
{
    /// <summary>
    /// Asynchronously saves a checkpoint of the current state for a specific flow run.
    /// </summary>
    /// <param name="runId">The unique identifier of the flow run.</param>
    /// <param name="state">The state to be saved.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    ValueTask SaveAsync(Guid runId, TState state, CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously loads a previously saved checkpoint for a specific flow run.
    /// </summary>
    /// <param name="runId">The unique identifier of the flow run.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous load operation and contains the saved state, or null if no checkpoint exists.</returns>
    ValueTask<TState?> LoadAsync(Guid runId, CancellationToken ct = default);
}
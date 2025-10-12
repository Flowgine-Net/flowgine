namespace Flowgine.Abstractions;

/// <summary>
/// Defines a branching specification that determines the next node(s) to execute based on the current state.
/// </summary>
/// <typeparam name="TState">The type of state used to evaluate the branch condition.</typeparam>
public class BranchSpec<TState>
{
    /// <summary>
    /// A function that selects the next node(s) based on the current state/context.
    /// </summary>
    public required Func<TState, CancellationToken, ValueTask<IReadOnlyCollection<string>>> Path { get; init; }

    /// <summary>
    /// Optional mapping table from label to node name (primarily for visualization purposes).
    /// </summary>
    public IReadOnlyDictionary<string, string>? PathMap { get; init; }
}
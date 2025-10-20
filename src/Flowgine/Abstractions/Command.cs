namespace Flowgine.Abstractions;

/// <summary>
/// Represents a command that encapsulates navigation instructions and state updates within a flow.
/// Provides type-safe navigation using <see cref="NodeRef{TState}"/>.
/// </summary>
public sealed class Command
{
    /// <summary>
    /// Gets the collection of target node IDs to navigate to (internal use).
    /// </summary>
    internal IReadOnlyCollection<string> GotoIds { get; }
    
    /// <summary>
    /// Gets the collection of state property updates as key-value pairs.
    /// </summary>
    public IReadOnlyCollection<(string Key, object? Value)> UpdateTuples { get; }

    /// <summary>
    /// Internal constructor for creating commands with string-based targets.
    /// </summary>
    private Command(IEnumerable<string> gotoIds, IEnumerable<(string, object?)>? updateTuples = null)
    {
        GotoIds = gotoIds.ToArray();
        UpdateTuples = (updateTuples ?? []).ToArray();
    }

    /// <summary>
    /// Creates a command that navigates to a single target node.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="target">The target node reference.</param>
    /// <returns>A new <see cref="Command"/> instance.</returns>
    public static Command Goto<TState>(NodeRef<TState> target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return new Command([target.Id]);
    }
    
    /// <summary>
    /// Creates a command that navigates to multiple target nodes (parallel execution).
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="targets">The target node references.</param>
    /// <returns>A new <see cref="Command"/> instance.</returns>
    public static Command Goto<TState>(params NodeRef<TState>[] targets)
    {
        if (targets == null || targets.Length == 0)
            throw new ArgumentException("At least one target required", nameof(targets));
        return new Command(targets.Select(t => t.Id));
    }
    
    /// <summary>
    /// Creates a command that navigates to multiple target nodes (parallel execution).
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="targets">The collection of target node references.</param>
    /// <returns>A new <see cref="Command"/> instance.</returns>
    public static Command Goto<TState>(IEnumerable<NodeRef<TState>> targets)
    {
        var targetArray = targets?.ToArray() ?? throw new ArgumentNullException(nameof(targets));
        if (targetArray.Length == 0)
            throw new ArgumentException("At least one target required", nameof(targets));
        return new Command(targetArray.Select(t => t.Id));
    }

    /// <summary>
    /// Creates a command that navigates to a target node and updates state properties.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="target">The target node reference.</param>
    /// <param name="updates">Dictionary of property updates.</param>
    /// <returns>A new <see cref="Command"/> instance with navigation and updates.</returns>
    public static Command GotoAndUpdate<TState>(
        NodeRef<TState> target, 
        IReadOnlyDictionary<string, object?> updates)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(updates);
        return new Command([target.Id], updates.Select(kv => (kv.Key, kv.Value)));
    }
    
    /// <summary>
    /// Creates a command that navigates to a target node and updates state properties.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="target">The target node reference.</param>
    /// <param name="updates">Tuple array of property updates.</param>
    /// <returns>A new <see cref="Command"/> instance with navigation and updates.</returns>
    public static Command GotoAndUpdate<TState>(
        NodeRef<TState> target,
        params (string Key, object? Value)[] updates)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(updates);
        return new Command([target.Id], updates);
    }

    /// <summary>
    /// Creates a command that only updates state properties without navigation.
    /// </summary>
    /// <param name="updates">Dictionary of property updates.</param>
    /// <returns>A new <see cref="Command"/> instance with only state updates.</returns>
    public static Command UpdateOnly(IReadOnlyDictionary<string, object?> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);
        return new Command([], updates.Select(kv => (kv.Key, kv.Value)));
    }
}

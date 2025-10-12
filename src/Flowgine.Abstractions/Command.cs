namespace Flowgine.Abstractions;

/// <summary>
/// Represents a command that encapsulates navigation instructions and state updates within a flow.
/// </summary>
public sealed class Command
{
    /// <summary>
    /// Gets the collection of target node names to navigate to.
    /// </summary>
    public IReadOnlyCollection<string> Goto { get; }
    
    /// <summary>
    /// Gets the collection of state property updates as key-value pairs.
    /// </summary>
    public IReadOnlyCollection<(string Key, object? Value)> UpdateTuples { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class with navigation targets and optional state updates.
    /// </summary>
    /// <param name="gotoTargets">The collection of target node names to navigate to.</param>
    /// <param name="updateTuples">Optional collection of state updates as key-value tuples.</param>
    public Command(IEnumerable<string> gotoTargets, IEnumerable<(string, object?)>? updateTuples = null)
    {
        Goto = gotoTargets.ToArray();
        UpdateTuples = (updateTuples ?? []).ToArray();
    }

    /// <summary>
    /// Creates a command that only specifies navigation targets without any state updates.
    /// </summary>
    /// <param name="targets">The target node names to navigate to.</param>
    /// <returns>A new <see cref="Command"/> instance with only navigation targets.</returns>
    public static Command GotoOnly(params string[] targets) => new(targets);
    
    /// <summary>
    /// Creates a command that only specifies state updates without any navigation targets.
    /// </summary>
    /// <param name="dict">A dictionary containing the state property updates.</param>
    /// <returns>A new <see cref="Command"/> instance with only state updates.</returns>
    public static Command UpdateOnly(IReadOnlyDictionary<string, object?> dict)
        => new([], dict.Select(kv => (kv.Key, kv.Value)));
}
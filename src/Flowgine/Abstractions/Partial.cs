using System.Linq.Expressions;

namespace Flowgine.Abstractions;

/// <summary>
/// Represents a partial state update that can be applied to a state object.
/// Used to selectively update specific properties of the state without replacing the entire state.
/// </summary>
/// <typeparam name="TState">The type of state being updated.</typeparam>
public readonly struct Partial<TState>
{
    private readonly List<(string Key, object? Value)> _updates;

    /// <summary>
    /// Initializes a new instance of the <see cref="Partial{TState}"/> struct.
    /// </summary>
    public Partial() => _updates = new();
    
    /// <summary>
    /// Sets a specific property value in the partial update.
    /// </summary>
    /// <typeparam name="TProp">The type of the property being set.</typeparam>
    /// <param name="selector">An expression that selects the property to update (e.g., s => s.PropertyName).</param>
    /// <param name="value">The new value for the property.</param>
    /// <returns>The current <see cref="Partial{TState}"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the selector is not a simple property access expression.</exception>
    public Partial<TState> Set<TProp>(Expression<Func<TState, TProp>> selector, TProp value)
    {
        // Extract the property name from the expression
        if (selector.Body is MemberExpression m)
        {
            _updates.Add((m.Member.Name, value));
            return this;
        }
        throw new ArgumentException("Selector must be a simple property access, e.g., s => s.X");
    }
    
    /// <summary>
    /// Converts the partial update to a list of key-value tuples.
    /// </summary>
    /// <returns>A read-only list of property name and value pairs.</returns>
    public IReadOnlyList<(string Key, object? Value)> ToTuples() => _updates;
}

/// <summary>
/// Factory class for creating partial state updates.
/// </summary>
public static class Update
{
    /// <summary>
    /// Creates a new partial update builder for the specified state type.
    /// </summary>
    /// <typeparam name="TState">The type of state to create a partial update for.</typeparam>
    /// <returns>A new <see cref="Partial{TState}"/> instance.</returns>
    public static Partial<TState> Of<TState>() => new();
}
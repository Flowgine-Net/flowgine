using System.Linq.Expressions;

namespace Flowgine.Abstractions;

public readonly struct Partial<TState>
{
    private readonly List<(string Key, object? Value)> _updates;

    public Partial() => _updates = new();
    
    public Partial<TState> Set<TProp>(Expression<Func<TState, TProp>> selector, TProp value)
    {
        // vytáhni název property z expressionu
        if (selector.Body is MemberExpression m)
        {
            _updates.Add((m.Member.Name, value));
            return this;
        }
        throw new ArgumentException("Selector must be a simple property access, e.g., s => s.X");
    }
    
    public IReadOnlyList<(string Key, object? Value)> ToTuples() => _updates;
}

// Factory pro Partial<T>
public static class Update
{
    public static Partial<TState> Of<TState>() => new();
}
using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._01_Basics;

public class MapNode : INode<AgentState>
{
    public object? Invoke(AgentState state, CancellationToken ct = default)
    {
        var newValue = state.X * (1 - state.X);

        return Update.Of<AgentState>()
            .Set(s => s.X, newValue);
    }
}

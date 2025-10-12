using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._03_LoopingFlow;

public class RandomNode : INode<AgentState>
{
    private static readonly Random _random = new();
    public object? Invoke(AgentState state, CancellationToken ct = default)
    {
        int newValue = _random.Next(0, 10);
        state.Numbers.Add(newValue);

        return Update.Of<AgentState>()
            .Set(s => s.Counter, state.Counter + 1)
            .Set(s => s.Numbers, state.Numbers);
    }
}

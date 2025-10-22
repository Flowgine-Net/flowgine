using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._03_LoopingFlow;

public class GreetingNode : INode<AgentState>
{
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        return Update.Of<AgentState>()
            .Set(s => s.Name, $"Hello, {state.Name}!")
            .Set(s => s.Counter, 0);
    }
}

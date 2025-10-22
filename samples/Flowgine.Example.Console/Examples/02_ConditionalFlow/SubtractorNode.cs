using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._02_ConditionalFlow;

public class SubtractorNode : INode<AgentState>
{
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        return Update.Of<AgentState>()
            .Set(s => s.Result, state.Number1 - state.Number2);
    }
}

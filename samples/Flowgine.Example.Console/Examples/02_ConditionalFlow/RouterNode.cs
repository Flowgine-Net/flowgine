using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._02_ConditionalFlow;

public class RouterNode : INode<AgentState>
{
    public object? Invoke(AgentState state, CancellationToken ct = default)
    {
        if (state.Operation == "+")
        {
            return Command.GotoOnly(nameof(AdderNode));
        }

        return Command.GotoOnly(nameof(SubtractorNode));
    }
}

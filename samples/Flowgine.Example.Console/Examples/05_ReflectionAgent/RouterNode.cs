using Flowgine.Abstractions;
using Flowgine.Abstractions.Helpers;

namespace Flowgine.Example.Console.Examples._05_ReflectionAgent;

public class RouterNode : INode<AgentState>
{
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        if (state.Messages.Count > 6)
        {
            return Command.GotoOnly(FlowgineEdge.END);
        }

        return Command.GotoOnly(nameof(ReflectionNode));
    }
}

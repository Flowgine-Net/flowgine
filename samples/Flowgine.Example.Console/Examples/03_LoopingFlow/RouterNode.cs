using Flowgine.Abstractions;
using Flowgine.Abstractions.Helpers;

namespace Flowgine.Example.Console.Examples._03_LoopingFlow;

public class RouterNode : INode<AgentState>
{
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        if (state.Counter < 5)
        {
            return Command.GotoOnly(nameof(RandomNode));
        }

        return Command.GotoOnly(FlowgineEdge.END);
    }
}

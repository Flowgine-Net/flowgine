using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._03_LoopingFlow;

public class RouterNode : INode<AgentState>
{
    private readonly NodeRef<AgentState> _randomNode;
    
    public RouterNode(NodeRef<AgentState> randomNode)
    {
        _randomNode = randomNode;
    }
    
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        if (state.Counter < 5)
        {
            return Command.Goto(_randomNode);
        }

        return Command.Goto(FlowBoundary<AgentState>.End);
    }
}

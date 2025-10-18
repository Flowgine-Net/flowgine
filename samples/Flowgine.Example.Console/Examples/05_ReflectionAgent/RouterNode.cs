using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._05_ReflectionAgent;

public class RouterNode : INode<AgentState>
{
    private readonly NodeRef<AgentState> _reflectionNode;
    
    public RouterNode(NodeRef<AgentState> reflectionNode)
    {
        _reflectionNode = reflectionNode;
    }
    
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        if (state.Messages.Count > 6)
        {
            return Command.Goto(FlowBoundary<AgentState>.End);
        }

        return Command.Goto(_reflectionNode);
    }
}

using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._02_ConditionalFlow;

public class RouterNode : INode<AgentState>
{
    private readonly NodeRef<AgentState> _adder;
    private readonly NodeRef<AgentState> _subtractor;
    
    public RouterNode(NodeRef<AgentState> adder, NodeRef<AgentState> subtractor)
    {
        _adder = adder;
        _subtractor = subtractor;
    }
    
    public object? Invoke(AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        if (state.Operation == "+")
        {
            return Command.Goto(_adder);
        }

        return Command.Goto(_subtractor);
    }
}

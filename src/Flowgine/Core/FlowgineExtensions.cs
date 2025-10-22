using Flowgine.Abstractions;

namespace Flowgine.Core;

/// <summary>
/// Extension methods for more ergonomic flow graph construction.
/// </summary>
public static class FlowgineExtensions
{
    /// <summary>
    /// Adds a node and immediately connects it to a previous node.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <typeparam name="TNode">The type of node being added.</typeparam>
    /// <param name="flow">The flow graph builder.</param>
    /// <param name="from">The source node reference to connect from.</param>
    /// <param name="node">The node instance to add.</param>
    /// <param name="name">Optional custom name for the node.</param>
    /// <returns>A strongly-typed reference to the newly added node.</returns>
    public static NodeRef<TState> Then<TState, TNode>(
        this Flowgine<TState> flow,
        NodeRef<TState> from,
        TNode node,
        string? name = null)
        where TNode : INode<TState>
    {
        var nodeRef = flow.AddNode(node, name);
        flow.AddEdge(from, nodeRef);
        return nodeRef;
    }
    
    /// <summary>
    /// Chains multiple nodes in a linear sequence from START to END.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="flow">The flow graph builder.</param>
    /// <param name="nodes">The nodes to chain in sequence.</param>
    /// <returns>A reference to the last node in the chain.</returns>
    /// <exception cref="ArgumentException">Thrown when no nodes are provided.</exception>
    public static NodeRef<TState> Chain<TState>(
        this Flowgine<TState> flow,
        params INode<TState>[] nodes)
    {
        if (nodes.Length == 0)
            throw new ArgumentException("At least one node required", nameof(nodes));
            
        var refs = nodes.Select(n => flow.AddNode(n)).ToArray();
        
        // Connect START to first
        flow.SetEntryPoint(refs[0]);
        
        // Chain them
        for (int i = 0; i < refs.Length - 1; i++)
        {
            flow.AddEdge(refs[i], refs[i + 1]);
        }
        
        // Connect last to END
        flow.SetFinishPoint(refs[^1]);
        
        return refs[^1];
    }
}


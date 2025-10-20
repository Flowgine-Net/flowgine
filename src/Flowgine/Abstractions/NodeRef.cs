namespace Flowgine.Abstractions;

/// <summary>
/// Strongly-typed reference to a node in a flow graph.
/// Eliminates string-based node identification errors and provides compile-time safety.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public sealed class NodeRef<TState>
{
    /// <summary>
    /// Unique identifier for this node within the graph.
    /// </summary>
    internal string Id { get; }
    
    /// <summary>
    /// The actual node instance.
    /// </summary>
    internal INode<TState> Node { get; }
    
    /// <summary>
    /// Human-readable name for debugging and visualization.
    /// </summary>
    public string Name { get; }
    
    internal NodeRef(string id, INode<TState> node, string name)
    {
        Id = id;
        Node = node;
        Name = name;
    }
    
    /// <summary>
    /// Returns the display name of the node for debugging purposes.
    /// </summary>
    public override string ToString() => Name;
}

/// <summary>
/// Special node references representing flow graph entry and exit points.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public static class FlowBoundary<TState>
{
    /// <summary>
    /// Reference to the flow graph entry point.
    /// Use this with <see cref="Flowgine.Core.Flowgine{TState}.AddEdge"/> to define the starting node.
    /// </summary>
    public static readonly NodeRef<TState> Start = 
        new("__start__", null!, "START");
    
    /// <summary>
    /// Reference to the flow graph exit point.
    /// Use this with <see cref="Flowgine.Core.Flowgine{TState}.AddEdge"/> or return from nodes to terminate execution.
    /// </summary>
    public static readonly NodeRef<TState> End = 
        new("__end__", null!, "END");
}


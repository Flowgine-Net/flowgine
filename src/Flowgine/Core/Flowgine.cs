using Flowgine.Abstractions;
using System.Collections.Generic;

namespace Flowgine.Core;

/// <summary>
/// Represents a flow graph builder that allows defining nodes and edges.
/// This class provides a fluent, type-safe API for constructing executable workflows.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public class Flowgine<TState>
{
    private readonly Dictionary<string, INode<TState>> _nodes = 
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _edges = 
        new(StringComparer.OrdinalIgnoreCase);
    
    private bool _compiled;

    /// <summary>
    /// Adds a node to the flow graph with automatic or explicit naming.
    /// Returns a strongly-typed reference that can be used for type-safe edge creation.
    /// </summary>
    /// <typeparam name="TNode">The type of the node being added.</typeparam>
    /// <param name="node">The node instance to add.</param>
    /// <param name="name">Optional custom name. If null, generates unique name from type.</param>
    /// <returns>A strongly-typed reference to the added node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the node parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a node with the same name already exists.</exception>
    public NodeRef<TState> AddNode<TNode>(TNode node, string? name = null) 
        where TNode : INode<TState>
    {
        ArgumentNullException.ThrowIfNull(node);
        
        if (_compiled)
        {
            WarnCompiled("nodes");
        }

        // Generate unique ID
        string nodeName;
        if (!string.IsNullOrWhiteSpace(name))
        {
            // User-specified name
            nodeName = name;
        }
        else
        {
            // Auto-generate: TypeName or TypeName_2, TypeName_3, etc.
            var baseName = typeof(TNode).Name;
            nodeName = baseName;
            int suffix = 2;
            
            while (_nodes.ContainsKey(nodeName))
            {
                nodeName = $"{baseName}_{suffix++}";
            }
        }

        if (_nodes.ContainsKey(nodeName))
        {
            throw new InvalidOperationException(
                $"Node with name '{nodeName}' already exists. " +
                "Use a different name or omit the name parameter for auto-generation.");
        }

        _nodes.Add(nodeName, node);
        
        return new NodeRef<TState>(nodeName, node, nodeName);
    }

    /// <summary>
    /// Adds a directed edge between two nodes using strongly-typed references.
    /// </summary>
    /// <param name="from">The source node reference.</param>
    /// <param name="to">The target node reference.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when from or to is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the source or target node does not exist.</exception>
    public Flowgine<TState> AddEdge(NodeRef<TState> from, NodeRef<TState> to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        
        if (_compiled)
        {
            WarnCompiled("edges");
        }
        
        // Validate nodes exist (except for START/END boundaries)
        if (from.Id != "__start__" && !_nodes.ContainsKey(from.Id))
        {
            throw new InvalidOperationException(
                $"Source node '{from.Name}' not present in graph.");
        }
        
        if (to.Id != "__end__" && !_nodes.ContainsKey(to.Id))
        {
            throw new InvalidOperationException(
                $"Target node '{to.Name}' not present in graph.");
        }
        
        // Store edges as adjacency list for efficient lookup
        if (!_edges.TryGetValue(from.Id, out var targets))
        {
            targets = new List<string>();
            _edges[from.Id] = targets;
        }
        
        if (!targets.Contains(to.Id))
        {
            targets.Add(to.Id);
        }
        
        return this;
    }

    /// <summary>
    /// Validates the flow graph structure to ensure it meets all requirements for execution.
    /// </summary>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the graph is invalid, such as missing a START entry point
    /// or having edges to unknown nodes.
    /// </exception>
    public Flowgine<TState> Validate()
    {
        // Step 1: START must exist as a source
        if (!_edges.ContainsKey("__start__"))
        {
            throw new InvalidOperationException(
                "Graph must have an entry point: add at least one edge from FlowBoundary<TState>.Start to a node.");
        }

        // Step 2: Validate all edges point to existing nodes (except END)
        foreach (var (fromId, targets) in _edges)
        {
            if (fromId != "__start__" && !_nodes.ContainsKey(fromId))
                throw new InvalidOperationException($"Found edge starting at unknown node '{fromId}'");
            
            foreach (var toId in targets)
            {
                if (toId != "__end__" && !_nodes.ContainsKey(toId))
                    throw new InvalidOperationException($"Found edge ending at unknown node '{toId}'");
            }
        }

        _compiled = true;
        return this;
    }
    
    /// <summary>
    /// Sets the entry point of the flow graph by adding an edge from START to the specified node.
    /// </summary>
    /// <param name="nodeRef">The node reference that should be the entry point.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    public Flowgine<TState> SetEntryPoint(NodeRef<TState> nodeRef) => 
        AddEdge(FlowBoundary<TState>.Start, nodeRef);
    
    /// <summary>
    /// Sets the finish point of the flow graph by adding an edge from the specified node to END.
    /// </summary>
    /// <param name="nodeRef">The node reference that should connect to the end point.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    public Flowgine<TState> SetFinishPoint(NodeRef<TState> nodeRef) => 
        AddEdge(nodeRef, FlowBoundary<TState>.End);
    
    /// <summary>
    /// Compiles and validates the flow graph into an executable instance.
    /// </summary>
    /// <param name="checkpointer">Optional checkpoint store for persisting flow state during execution.</param>
    /// <param name="name">Optional name for the compiled flow. Defaults to "Flowgine".</param>
    /// <returns>A compiled and validated flow ready for execution.</returns>
    public CompiledFlowgine<TState> Compile(ICheckpointStore<TState>? checkpointer = null, string? name = null)
        => new(this, checkpointer, name ?? "Flowgine");

    /// <summary>
    /// Outputs a warning message when attempting to modify a graph that has already been compiled.
    /// </summary>
    /// <param name="what">A description of what is being added (e.g., "nodes", "edges").</param>
    private static void WarnCompiled(string what) =>
        Console.Error.WriteLine($"[Flowgine] Adding {what} to a graph that has already been compiled. It won't affect compiled instances.");
    
    /// <summary>
    /// Gets the adjacency list of edges in the flow graph (internal use).
    /// </summary>
    internal IReadOnlyDictionary<string, List<string>> Edges => _edges;
    
    /// <summary>
    /// Gets the dictionary of nodes in the flow graph (internal use).
    /// </summary>
    internal IReadOnlyDictionary<string, INode<TState>> Nodes => _nodes;
}

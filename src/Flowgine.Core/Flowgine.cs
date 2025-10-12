using Flowgine.Abstractions;
using Flowgine.Abstractions.Helpers;

namespace Flowgine.Core;

/// <summary>
/// Represents a flow graph builder that allows defining nodes, edges, and conditional branches.
/// This class provides a fluent API for constructing executable workflows.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public class Flowgine<TState>
{
    private readonly Dictionary<string, INode<TState>> _nodes = new();
    private readonly HashSet<(string From, string To)> _edges = new();
    private readonly Dictionary<string, List<BranchSpec<TState>>> _branches = new(StringComparer.OrdinalIgnoreCase);
    
    private bool _compiled;

    /// <summary>
    /// Adds a node to the flow graph.
    /// </summary>
    /// <param name="node">The node to add to the flow graph.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the node parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a node with the same name already exists in the graph.</exception>
    public Flowgine<TState> AddNode(INode<TState> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        
        if (_compiled)
        {
            WarnCompiled("nodes");
        }

        if (!_nodes.TryAdd(node.Name, node))
        {
            throw new InvalidOperationException($"Node `{node.Name}` already present.");
        }

        return this;
    }

    /// <summary>
    /// Adds a directed edge between two nodes in the flow graph.
    /// </summary>
    /// <param name="start">The name of the source node (or FlowgineEdge.START for the entry point).</param>
    /// <param name="end">The name of the target node (or FlowgineEdge.END for the exit point).</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the start or end node does not exist in the graph.</exception>
    public Flowgine<TState> AddEdge(string start, string end)
    {
        if (_compiled)
        {
            WarnCompiled("edges");
        }
        
        if(!_nodes.ContainsKey(start) && start != FlowgineEdge.START)
        {
            throw new InvalidOperationException($"Node `{start}` not present.");
        }
        
        if(!_nodes.ContainsKey(end) && end != FlowgineEdge.END)
        {
            throw new InvalidOperationException($"Node `{end}` not present.");
        }
        
        _edges.Add((start, end));
        
        return this;
    }
    
    /// <summary>
    /// Adds conditional branching logic to a node, allowing dynamic path selection based on the current state.
    /// </summary>
    /// <param name="source">The name of the source node from which the conditional branches originate.</param>
    /// <param name="path">A function that evaluates the current state and returns a collection of target node names.</param>
    /// <param name="pathMap">Optional mapping from labels to node names for visualization purposes.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the source node does not exist in the graph.</exception>
    public Flowgine<TState> AddConditionalEdges(
        string source,
        Func<TState, CancellationToken, ValueTask<IReadOnlyCollection<string>>> path,
        IReadOnlyDictionary<string, string>? pathMap = null)
    {
        if (_compiled)
        {
            WarnCompiled("edge");
        }

        if (!_nodes.ContainsKey(source))
        {
            throw new InvalidOperationException($"Need to AddNode `{source}` first");
        }
        
        if (!_branches.TryGetValue(source, out var list))
        {
            list = new List<BranchSpec<TState>>();
            _branches[source] = list;
        }
        list.Add(new BranchSpec<TState> { Path = path, PathMap = pathMap });
        
        return this;
    }

    /// <summary>
    /// Validates the flow graph structure to ensure it meets all requirements for execution.
    /// </summary>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the graph is invalid, such as missing a START entry point,
    /// having edges to unknown nodes, or having branches defined for non-existent nodes.
    /// </exception>
    public Flowgine<TState> Validate()
    {
        // Step 1: START must exist as a source
        var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var (from, _) in _edges) sources.Add(from);
        foreach (var kv in _branches) sources.Add(kv.Key);

        if (!sources.Contains(FlowgineEdge.START))
        {
            throw new InvalidOperationException(
                "Graph must have an entrypoint: add at least one edge from START to another node");
        }

        // Step 2: Every source (except START) and target (except END) must be a known node
        foreach (var (From, To) in _edges)
        {
            if (From != FlowgineEdge.START && !_nodes.ContainsKey(From))
                throw new InvalidOperationException($"Found edge starting at unknown node '{From}'");
            if (To != FlowgineEdge.END && !_nodes.ContainsKey(To))
                throw new InvalidOperationException($"Found edge ending at unknown node '{To}'");
        }
        foreach (var start in _branches.Keys)
        {
            if (!_nodes.ContainsKey(start))
                throw new InvalidOperationException($"Branch defined for unknown node '{start}'");
        }

        _compiled = true;
        return this;
    }
    
    /// <summary>
    /// Sets the entry point of the flow graph by adding an edge from START to the specified node.
    /// </summary>
    /// <param name="key">The name of the node that should be the entry point.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    public Flowgine<TState> SetEntryPoint(string key) => AddEdge(FlowgineEdge.START, key);
    
    /// <summary>
    /// Sets the finish point of the flow graph by adding an edge from the specified node to END.
    /// </summary>
    /// <param name="key">The name of the node that should connect to the end point.</param>
    /// <returns>The current <see cref="Flowgine{TState}"/> instance for method chaining.</returns>
    public Flowgine<TState> SetFinishPoint(string key) => AddEdge(key, FlowgineEdge.END);
    
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
    /// Gets the collection of edges in the flow graph.
    /// </summary>
    internal IReadOnlyCollection<(string From, string To)> Edges => _edges;
    
    /// <summary>
    /// Gets the dictionary of nodes in the flow graph.
    /// </summary>
    internal IReadOnlyDictionary<string, INode<TState>> Nodes => _nodes;
    
    /// <summary>
    /// Gets the dictionary of conditional branch specifications in the flow graph.
    /// </summary>
    internal IReadOnlyDictionary<string, List<BranchSpec<TState>>> Branches => _branches;
}
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

using Flowgine.Abstractions;
using Flowgine.Abstractions.Helpers;

namespace Flowgine.Core;

/// <summary>
/// Represents a compiled and validated flow that is ready for execution.
/// This class orchestrates the execution of nodes, manages state transitions, and handles checkpointing.
/// </summary>
/// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
public sealed class CompiledFlowgine<TState>
{
    /// <summary>
    /// Special key used to indicate a full state replacement instead of partial updates.
    /// </summary>
    private const string ROOT = "__root__";
    
    private readonly Flowgine<TState> _builder;
    private readonly ICheckpointStore<TState>? _checkpoints;
    
    /// <summary>
    /// Gets the name of this compiled flow.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledFlowgine{TState}"/> class.
    /// </summary>
    /// <param name="builder">The flow builder containing the node graph definition.</param>
    /// <param name="checkpoints">Optional checkpoint store for persisting flow state.</param>
    /// <param name="name">The name of the compiled flow.</param>
    internal CompiledFlowgine(
        Flowgine<TState> builder, 
        ICheckpointStore<TState>? checkpoints, 
        string name)
    {
        _builder = builder.Validate();
        _checkpoints = checkpoints;
        Name = name;
    }

    public async Task<TState> RunToCompletionAsync(
        TState initial,
        Guid runId,
        IServiceProvider? services = null,
        CancellationToken ct = default)
    {
        var final = initial;
        await foreach (var ev in RunAsync(initial, runId, services, ct))
            if (ev is NodeCompleted<TState> done)
                final = done.State;

        return final;
    }

    /// <summary>
    /// Asynchronously executes the compiled flow and yields events describing the execution progress.
    /// </summary>
    /// <param name="initialState">The initial state to start the flow execution with.</param>
    /// <param name="runId">A unique identifier for this specific flow run, used for checkpointing.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An async enumerable of flow events that describe the execution progress.</returns>
    public async IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState, 
        Guid runId,
        IServiceProvider? services = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Load checkpoint if available
        var state = _checkpoints is null 
            ? initialState : await _checkpoints.LoadAsync(runId, ct) ?? initialState;
        
        IServiceScope? scope = null;
        IServiceProvider sp = services ?? EmptyServiceProvider.Instance;

        if (sp.GetService(typeof(IServiceScopeFactory)) is IServiceScopeFactory sf)
        {
            scope = sf.CreateScope();
        }

        try
        {
            var runSp = scope?.ServiceProvider ?? sp;
            var runtime = new Runtime(runId, runSp);
            
            // Find the starting node
            var next = _builder.Edges.FirstOrDefault(e => e.From == FlowgineEdge.START).To;
            if (string.IsNullOrEmpty(next)) yield break;
            
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                
                yield return new NodeStarted<TState>(next);
                
                var node = _builder.Nodes[next];
                var result = await node.InvokeAsync(state, runtime, ct);
                
                // Normalize node output
                var (updates, commands) = NormalizeNodeResult(result);

                if (updates.Count > 0)
                {
                    state = ApplyUpdates(state, updates);
                    if (_checkpoints is not null)
                        await _checkpoints.SaveAsync(runId, state, ct);
                }
                
                yield return new NodeCompleted<TState>(next, state);
                
                // Step 3: Extract target nodes from Commands (goto)
                var gotoTargets = commands.SelectMany(c => c.Goto).ToList();
                
                // Step 4: If no Commands exist, try explicit edges/branch logic
                if (gotoTargets.Count == 0)
                {
                    // Check if there's a branch specification for the current node
                    if (_builder.Branches.TryGetValue(next, out var branchSpecs) && branchSpecs.Count > 0)
                    {
                        var acc = new List<string>(4);
                        foreach (var br in branchSpecs)
                        {
                            var chosen = await br.Path(state, ct);
                            if (chosen is { Count: > 0 }) acc.AddRange(chosen);
                        }
                        gotoTargets = acc;
                    }
                    // If branch logic didn't determine anything, use classic edge next->X (if it exists)
                    if (gotoTargets.Count == 0)
                    {
                        var edge = _builder.Edges.FirstOrDefault(e => e.From.Equals(next, StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(edge.To)) gotoTargets.Add(edge.To);
                    }
                }
                
                // Signal the chosen branches
                if (gotoTargets.Count > 0)
                    yield return new BranchTaken<TState>(next, gotoTargets);

                // Step 5: Select the next node based on gotoTargets (ignore END; if everything is END → finish)
                var firstNext = gotoTargets.FirstOrDefault(t => t != FlowgineEdge.END);
                if (string.IsNullOrEmpty(firstNext)) yield break;

                next = firstNext;
            }
        }
        finally
        {
            scope?.Dispose();
        }
    }

    /// <summary>
    /// Normalizes the result from a node invocation into a consistent format of updates and commands.
    /// Supports various result types including Command, IEnumerable&lt;Command&gt;, dictionaries, and direct state objects.
    /// </summary>
    /// <param name="result">The result returned from a node's InvokeAsync method.</param>
    /// <returns>A tuple containing state updates and navigation commands extracted from the result.</returns>
    private static (List<(string Key, object? Value)> Updates, List<Command> Commands) NormalizeNodeResult(
        object? result)
    {
        var updates = new List<(string, object?)>(4);
        var cmds = new List<Command>(2);

        if (result is null) return (updates, cmds);
        
        switch (result)
        {
            case Command c:
                cmds.Add(c);
                updates.AddRange(c.UpdateTuples);
                return (updates, cmds);

            case IEnumerable<Command> many:
                foreach (var c2 in many) { cmds.Add(c2); updates.AddRange(c2.UpdateTuples); }
                return (updates, cmds);

            case IReadOnlyDictionary<string, object?> dict:
                updates.AddRange(dict.Select(kv => (kv.Key, kv.Value)));
                return (updates, cmds);
            
            case Partial<TState> partial:
                updates.AddRange(partial.ToTuples());
                return (updates, cmds);

            default:
                // If the type matches TState, treat it as a "full state replacement" using the special __root__ key
                if (result is TState s)
                {
                    updates.Add(("__root__", s));
                    return (updates, cmds);
                }
                // Support for ValueTuple containing Commands (e.g., (Command, Command))
                if (result is System.Collections.IEnumerable seq)
                {
                    foreach (var item in seq)
                    {
                        if (item is Command c3) { cmds.Add(c3); updates.AddRange(c3.UpdateTuples); }
                        else if (item is IReadOnlyDictionary<string, object?> d2) updates.AddRange(d2.Select(kv => (kv.Key, kv.Value)));
                        else if (item is TState s2) updates.Add(("__root__", s2));
                    }
                    return (updates, cmds);
                }
                // Otherwise unknown type – leave as is (no update/command)
                return (updates, cmds);
        }
    }

    /// <summary>
    /// Applies a list of property updates to the current state.
    /// If a __root__ update exists, it replaces the entire state; otherwise, updates are applied to individual properties.
    /// </summary>
    /// <param name="current">The current state object.</param>
    /// <param name="updates">The list of property updates to apply.</param>
    /// <returns>The updated state object.</returns>
    private static TState ApplyUpdates(TState current, List<(string Key, object? Value)> updates)
    {
        var root = updates.FirstOrDefault(t => t.Key == ROOT);
        if (root != default)
        {
            return (TState?)root.Value ?? current;
        }
        
        // Object "patching" (only public settable properties)
        var type = typeof(TState);
        var dict = updates
            .Where(t => t.Key != ROOT)
            .GroupBy(t => t.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.OrdinalIgnoreCase);

        // If TState is a record with init-only properties, patching won't work – return current
        // For simpler implementation, try mutable properties:
        var props = type.GetProperties().Where(p => p.CanWrite);
        var clone = current;
        foreach (var p in props)
        {
            if (dict.TryGetValue(p.Name, out var v))
            {
                // Simple conversion – in production, type mapping/JSON could be added
                if (v is null && p.PropertyType.IsValueType && Nullable.GetUnderlyingType(p.PropertyType) is null)
                    continue;
                try { p.SetValue(clone, v); } catch { /* best-effort */ }
            }
        }
        
        return clone;
    }
}
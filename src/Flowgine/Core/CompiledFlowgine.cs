using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

using Flowgine.Abstractions;

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
    /// Maximum number of iterations before the flow is considered to be in an infinite loop.
    /// </summary>
    public int MaxIterations { get; init; } = 1000;
    
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

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public Task<TState> RunToCompletionAsync(
        TState initial,
        CancellationToken ct = default)
        => RunToCompletionAsync(initial, services: null, Guid.NewGuid(), ct);

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="services">Optional service provider for dependency injection.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public Task<TState> RunToCompletionAsync(
        TState initial,
        IServiceProvider? services,
        CancellationToken ct = default)
        => RunToCompletionAsync(initial, services, Guid.NewGuid(), ct);

    /// <summary>
    /// Executes the flow to completion and returns the final state.
    /// </summary>
    /// <param name="initial">The initial state to start execution with.</param>
    /// <param name="services">Optional service provider for dependency injection.</param>
    /// <param name="runId">A unique identifier for this specific flow run, used for checkpointing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final state after flow completion.</returns>
    public async Task<TState> RunToCompletionAsync(
        TState initial,
        IServiceProvider? services,
        Guid runId,
        CancellationToken ct = default)
    {
        var final = initial;
        await foreach (var ev in RunAsync(initial, services, runId, ct))
            if (ev is NodeCompleted<TState> done)
                final = done.State;

        return final;
    }

    /// <summary>
    /// Asynchronously executes the compiled flow and yields events describing the execution progress.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initialState">The initial state to start the flow execution with.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An async enumerable of flow events that describe the execution progress.</returns>
    public IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState,
        CancellationToken ct = default)
        => RunAsync(initialState, services: null, Guid.NewGuid(), ct);

    /// <summary>
    /// Asynchronously executes the compiled flow and yields events describing the execution progress.
    /// A unique run ID is automatically generated.
    /// </summary>
    /// <param name="initialState">The initial state to start the flow execution with.</param>
    /// <param name="services">Optional service provider for dependency injection in nodes.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An async enumerable of flow events that describe the execution progress.</returns>
    public IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState,
        IServiceProvider? services,
        CancellationToken ct = default)
        => RunAsync(initialState, services, Guid.NewGuid(), ct);

    /// <summary>
    /// Asynchronously executes the compiled flow and yields events describing the execution progress.
    /// </summary>
    /// <param name="initialState">The initial state to start the flow execution with.</param>
    /// <param name="services">Optional service provider for dependency injection in nodes.</param>
    /// <param name="runId">A unique identifier for this specific flow run, used for checkpointing and tracing.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An async enumerable of flow events that describe the execution progress.</returns>
    public async IAsyncEnumerable<FlowgineEvent<TState>> RunAsync(
        TState initialState,
        IServiceProvider? services,
        Guid runId,
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
            if (!_builder.Edges.TryGetValue("__start__", out var startTargets) || startTargets.Count == 0)
            {
                yield break;
            }
            
            var next = startTargets[0];
            int iterationCount = 0;
            
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                
                // Check for infinite loops
                if (++iterationCount > MaxIterations)
                {
                    throw new InvalidOperationException(
                        $"Flow exceeded maximum iterations ({MaxIterations}). " +
                        "Possible infinite loop detected. Consider increasing MaxIterations if this is intentional.");
                }
                
                yield return new NodeStarted<TState>(next);
                
                var node = _builder.Nodes[next];
                object? result;
                Exception? failureException = null;
                
                try
                {
                    result = await node.InvokeAsync(state, runtime, ct);
                }
                catch (Exception ex)
                {
                    failureException = ex;
                    result = null;
                }
                
                // If node failed, yield failure event and rethrow
                if (failureException != null)
                {
                    yield return new NodeFailed<TState>(next, state, failureException);
                    throw failureException;
                }
                
                // Normalize node output
                var (updates, commands) = NormalizeNodeResult(result);

                if (updates.Count > 0)
                {
                    state = ApplyUpdates(state, updates);
                    if (_checkpoints is not null)
                        await _checkpoints.SaveAsync(runId, state, ct);
                }
                
                yield return new NodeCompleted<TState>(next, state);
                
                // Extract target nodes from Commands (goto)
                var gotoTargets = commands.SelectMany(c => c.GotoIds).ToList();
                
                // If no Commands exist, try explicit edges
                if (gotoTargets.Count == 0)
                {
                    if (_builder.Edges.TryGetValue(next, out var edgeTargets))
                    {
                        gotoTargets.AddRange(edgeTargets);
                    }
                }
                
                // Signal the chosen branches
                if (gotoTargets.Count > 0)
                    yield return new BranchTaken<TState>(next, gotoTargets);

                // Select the next node (ignore END; if everything is END → finish)
                var firstNext = gotoTargets.FirstOrDefault(t => t != "__end__");
                if (string.IsNullOrEmpty(firstNext)) 
                    yield break;

                // Validate that the target node exists before continuing
                if (!_builder.Nodes.ContainsKey(firstNext))
                {
                    var availableNodes = string.Join(", ", _builder.Nodes.Keys);
                    throw new InvalidOperationException(
                        $"Node '{next}' attempted to navigate to unknown node '{firstNext}'. " +
                        $"Available nodes: {availableNodes}");
                }

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
    /// </summary>
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
                foreach (var c2 in many) 
                { 
                    cmds.Add(c2); 
                    updates.AddRange(c2.UpdateTuples); 
                }
                return (updates, cmds);

            case IReadOnlyDictionary<string, object?> dict:
                updates.AddRange(dict.Select(kv => (kv.Key, kv.Value)));
                return (updates, cmds);
            
            case Partial<TState> partial:
                updates.AddRange(partial.ToTuples());
                return (updates, cmds);

            default:
                // If the type matches TState, treat it as a "full state replacement"
                if (result is TState s)
                {
                    updates.Add((ROOT, s));
                    return (updates, cmds);
                }
                
                // Support for sequences containing Commands
                if (result is System.Collections.IEnumerable seq)
                {
                    foreach (var item in seq)
                    {
                        if (item is Command c3) 
                        { 
                            cmds.Add(c3); 
                            updates.AddRange(c3.UpdateTuples); 
                        }
                        else if (item is IReadOnlyDictionary<string, object?> d2) 
                            updates.AddRange(d2.Select(kv => (kv.Key, kv.Value)));
                        else if (item is TState s2) 
                            updates.Add((ROOT, s2));
                    }
                    return (updates, cmds);
                }
                
                return (updates, cmds);
        }
    }

    /// <summary>
    /// Applies a list of property updates to the current state.
    /// </summary>
    private static TState ApplyUpdates(TState current, List<(string Key, object? Value)> updates)
    {
        var rootUpdate = updates.FirstOrDefault(t => t.Key == ROOT);
        if (!string.IsNullOrEmpty(rootUpdate.Key))
        {
            return (TState?)rootUpdate.Value ?? current;
        }
        
        // Object "patching" (only public settable properties)
        var type = typeof(TState);
        var dict = updates
            .Where(t => t.Key != ROOT)
            .GroupBy(t => t.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.OrdinalIgnoreCase);

        var writableProps = type.GetProperties().Where(p => p.CanWrite).ToList();
        
        if (writableProps.Count > 0)
        {
            var clone = current;
            foreach (var p in writableProps)
            {
                if (dict.TryGetValue(p.Name, out var v))
                {
                    if (v is null && p.PropertyType.IsValueType && 
                        Nullable.GetUnderlyingType(p.PropertyType) is null)
                        continue;
                        
                    try 
                    { 
                        p.SetValue(clone, v); 
                    } 
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to set property '{p.Name}' on type '{type.Name}'. " +
                            $"Value type: {v?.GetType().Name ?? "null"}. " +
                            $"Expected type: {p.PropertyType.Name}.", ex);
                    }
                }
            }
            return clone;
        }
        
        // For immutable types (records), try constructor-based creation
        var ctors = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .ToList();

        List<Exception>? constructorErrors = null;
            
        foreach (var ctor in ctors)
        {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            bool allMatched = true;
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var paramName = param.Name;
                
                if (paramName != null && dict.TryGetValue(paramName, out var newValue))
                {
                    args[i] = newValue;
                }
                else
                {
                    // Try to get from current state
                    var prop = type.GetProperty(paramName!, 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.IgnoreCase);
                    
                    if (prop != null)
                    {
                        args[i] = prop.GetValue(current);
                    }
                    else if (param.HasDefaultValue)
                    {
                        args[i] = param.DefaultValue;
                    }
                    else
                    {
                        allMatched = false;
                        break;
                    }
                }
            }
            
            if (allMatched)
            {
                try
                {
                    return (TState)ctor.Invoke(args);
                }
                catch (Exception ex)
                {
                    // Collect errors for reporting
                    constructorErrors ??= new List<Exception>();
                    constructorErrors.Add(ex);
                }
            }
        }
        
        // If we tried constructors but all failed, throw detailed error
        if (constructorErrors != null && constructorErrors.Count > 0)
        {
            var errorDetails = string.Join("; ", constructorErrors.Select(e => e.Message));
            throw new InvalidOperationException(
                $"Failed to update immutable type '{type.Name}'. " +
                $"Tried {constructorErrors.Count} constructor(s) but all failed. " +
                $"Updates: {string.Join(", ", dict.Keys)}. " +
                $"Errors: {errorDetails}", 
                constructorErrors[0]);
        }
        
        // If we have updates but no way to apply them, warn
        if (dict.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot apply updates to type '{type.Name}'. " +
                $"Type has no writable properties and no suitable constructor found. " +
                $"Attempted updates: {string.Join(", ", dict.Keys)}.");
        }
        
        // No updates to apply - return current unchanged
        return current;
    }
}

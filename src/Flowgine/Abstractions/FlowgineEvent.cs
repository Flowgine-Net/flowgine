namespace Flowgine.Abstractions;

/// <summary>
/// Represents a base class for events in the Flowgine framework, encapsulating stateful operations during a flow execution.
/// </summary>
/// <typeparam name="TState">The type of state associated with the event.</typeparam>
public abstract record FlowgineEvent<TState>;

/// <summary>
/// Represents an event that signifies the start of a specific node within a flow execution.
/// </summary>
/// <typeparam name="TState">The type of state associated with the flow operation.</typeparam>
/// <param name="NodeName">The name of the node that has started.</param>
/// <param name="Metadata">Optional metadata associated with the node (e.g., observation type for tracing).</param>
public sealed record NodeStarted<TState>(
    string NodeName, 
    IReadOnlyDictionary<string, string>? Metadata = null
) : FlowgineEvent<TState>;

/// <summary>
/// Represents an event that signifies the completion of a specific node within a flow execution.
/// </summary>
/// <typeparam name="TState">The type of state associated with the flow operation.</typeparam>
/// <param name="NodeName">The name of the node that has completed.</param>
/// <param name="State">The state associated with the flow operation at the time of node completion.</param>
public sealed record NodeCompleted<TState>(string NodeName, TState State) : FlowgineEvent<TState>;

/// <summary>
/// Represents an event that indicates a node execution has failed with an exception.
/// </summary>
/// <typeparam name="TState">The type of state associated with the flow operation.</typeparam>
/// <param name="NodeName">The name of the node that failed.</param>
/// <param name="State">The state at the time of failure.</param>
/// <param name="Error">The exception that caused the failure.</param>
public sealed record NodeFailed<TState>(string NodeName, TState State, Exception Error) : FlowgineEvent<TState>;

/// <summary>
/// Represents an event that indicates a branch transition within a flow execution, capturing the originating node and the potential destination nodes.
/// </summary>
/// <typeparam name="TState">The type of state associated with the flow operation.</typeparam>
/// <param name="From">The name of the originating node from which the branch transition occurs.</param>
/// <param name="To">The collection of node names to which the branch transition may lead.</param>
public sealed record BranchTaken<TState>(string From, IReadOnlyCollection<string> To) : FlowgineEvent<TState>;

/// <summary>
/// Represents an event that indicates a flow execution has been interrupted, providing the reason for the interruption.
/// </summary>
/// <typeparam name="TState">The type of state associated with the flow operation.</typeparam>
/// <param name="Reason">A description of the reason why the interruption occurred.</param>
public sealed record Interrupted<TState>(string Reason) : FlowgineEvent<TState>;
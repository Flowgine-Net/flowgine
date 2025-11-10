namespace Flowgine.Abstractions;

/// <summary>
/// Marker interface to specify Langfuse observation type for a node.
/// </summary>
public interface IObservableNode
{
    /// <summary>
    /// Gets the Langfuse observation type for this node.
    /// Possible values: "agent", "tool", "chain", "retriever", "evaluator", "event", "span"
    /// </summary>
    string ObservationType { get; }
}
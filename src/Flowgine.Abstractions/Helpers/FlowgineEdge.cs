namespace Flowgine.Abstractions.Helpers;

/// <summary>
/// Provides reserved identifiers for graph structures,
/// specifically for marking the start and end points.
/// </summary>
public static class FlowgineEdge
{
    /// <summary>
    /// Represents the reserved identifier for the starting point in a graph structure.
    /// This constant is used as a marker for the initial node or step in a graph-based system.
    /// </summary>
    public const string START = "__start__";
    
    /// <summary>
    /// Represents the reserved identifier for the ending point in a graph structure.
    /// This constant is used as a marker for the final node or step in a graph-based system.
    /// </summary>
    public const string END   = "__end__";
}
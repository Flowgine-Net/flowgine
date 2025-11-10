namespace Flowgine.Observability;

/// <summary>
/// Interface for states that can provide observable input/output for tracing.
/// </summary>
public interface IObservableState
{
    /// <summary>
    /// Gets the input value to be logged at the start of trace.
    /// </summary>
    string? GetInput();
    
    /// <summary>
    /// Gets the output value to be logged at the end of trace.
    /// </summary>
    string? GetOutput();
}


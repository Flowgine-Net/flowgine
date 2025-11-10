namespace Flowgine.Observability;

/// <summary>
/// Simple implementation of observable state with string input/output properties.
/// Can be used as a base class or composed into custom state types.
/// </summary>
public class ObservableState
{
    /// <summary>
    /// Gets or sets the input value for the trace.
    /// </summary>
    public string? Input { get; set; }
    
    /// <summary>
    /// Gets or sets the output value for the trace.
    /// </summary>
    public string? Output { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableState"/> class.
    /// </summary>
    public ObservableState()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableState"/> class with an input value.
    /// </summary>
    /// <param name="input">The initial input value.</param>
    public ObservableState(string? input)
    {
        Input = input;
    }
}


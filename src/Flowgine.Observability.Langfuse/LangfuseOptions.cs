namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Configuration options for Langfuse observability integration.
/// </summary>
public class LangfuseOptions
{
    /// <summary>
    /// Gets or sets the Langfuse public API key.
    /// </summary>
    public required string PublicKey { get; set; } 
    
    /// <summary>
    /// Gets or sets the Langfuse secret API key.
    /// </summary>
    public required string SecretKey { get; set; } 
    
    /// <summary>
    /// Gets or sets the application name used for identifying traces in Langfuse.
    /// </summary>
    public required string ApplicationName { get; set; } 
    
    /// <summary>
    /// Gets or sets the Langfuse API endpoint.
    /// Default is "https://cloud.langfuse.com" for Langfuse Cloud.
    /// For self-hosted instances, provide your custom endpoint.
    /// </summary>
    public string LangfuseHost { get; set; } = "https://cloud.langfuse.com"; 
    
    /// <summary>
    /// Gets or sets the deployment environment name (e.g., "production", "staging", "development").
    /// Default is "production".
    /// </summary>
    public string? Environment { get; set; } = "production"; 
    
    /// <summary>
    /// Gets or sets the timeout for OTLP export operations in milliseconds.
    /// Default is 30000 (30 seconds).
    /// </summary>
    public int TimeoutMs { get; set; } = 30000; 
    
    /// <summary>
    /// Gets or sets whether to enable console output for debugging traces.
    /// When true, trace data will be printed to the console in addition to being sent to Langfuse.
    /// Default is false.
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;
}
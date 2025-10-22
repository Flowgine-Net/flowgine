namespace Flowgine.LLM.OpenAI;

/// <summary>
/// Configuration options for OpenAI chat services.
/// </summary>
public sealed class OpenAIChatOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = "";
    
    /// <summary>
    /// Gets or sets a custom base URL for the OpenAI API endpoint.
    /// Use this for proxies, Azure OpenAI, or local LLM servers.
    /// </summary>
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the default model identifier to use for chat completions.
    /// Default is "gpt-4o-mini".
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4o-mini";
    
    /// <summary>
    /// Gets or sets the default temperature for controlling response randomness.
    /// Range: 0.0 to 2.0. Lower values produce more deterministic responses.
    /// </summary>
    public float? DefaultTemperature { get; set; }
    
    /// <summary>
    /// Gets or sets the default maximum number of tokens to generate in responses.
    /// </summary>
    public int? DefaultMaxTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the timeout for API requests.
    /// If not specified, the default timeout from the OpenAI SDK will be used.
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// Default is 3. Set to 0 to disable retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// Actual delay uses exponential backoff: delay * 2^(attempt-1).
    /// If not specified, a default of 1 second will be used.
    /// </summary>
    public TimeSpan? RetryDelay { get; set; }
}
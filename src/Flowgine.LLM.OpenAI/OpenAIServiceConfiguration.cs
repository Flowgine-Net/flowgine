namespace Flowgine.LLM.OpenAI;

/// <summary>
/// Fluent configuration builder for OpenAI services.
/// </summary>
public sealed class OpenAIServiceConfiguration
{
    internal OpenAIChatOptions Options { get; } = new();
    
    /// <summary>
    /// Sets the OpenAI API key for authentication.
    /// </summary>
    /// <param name="apiKey">The API key for OpenAI or compatible service</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentException">Thrown when apiKey is null or whitespace</exception>
    public OpenAIServiceConfiguration UseApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
            
        Options.ApiKey = apiKey;
        return this;
    }
    
    /// <summary>
    /// Sets a custom endpoint/base URL for the OpenAI API.
    /// Use this for Azure OpenAI, proxies, or local LLM servers.
    /// </summary>
    /// <param name="baseUrl">The base URL for the API endpoint</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentException">Thrown when baseUrl is null or whitespace</exception>
    public OpenAIServiceConfiguration UseEndpoint(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
            
        Options.BaseUrl = baseUrl;
        return this;
    }
    
    /// <summary>
    /// Sets the default model to use for chat completions.
    /// </summary>
    /// <param name="model">The model identifier (e.g., "gpt-4o-mini", "gpt-4")</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentException">Thrown when model is null or whitespace</exception>
    public OpenAIServiceConfiguration UseModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be null or empty.", nameof(model));
            
        Options.DefaultModel = model;
        return this;
    }
    
    /// <summary>
    /// Sets the default temperature for chat completions (controls randomness).
    /// </summary>
    /// <param name="temperature">Temperature value between 0.0 and 2.0</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when temperature is not between 0 and 2</exception>
    public OpenAIServiceConfiguration UseTemperature(float temperature)
    {
        if (temperature < 0.0f || temperature > 2.0f)
            throw new ArgumentOutOfRangeException(nameof(temperature), 
                "Temperature must be between 0.0 and 2.0.");
            
        Options.DefaultTemperature = temperature;
        return this;
    }
    
    /// <summary>
    /// Sets the default maximum number of tokens to generate.
    /// </summary>
    /// <param name="maxTokens">Maximum tokens for completion</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxTokens is less than 1</exception>
    public OpenAIServiceConfiguration UseMaxTokens(int maxTokens)
    {
        if (maxTokens < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), 
                "MaxTokens must be at least 1.");
            
        Options.DefaultMaxTokens = maxTokens;
        return this;
    }
    
    /// <summary>
    /// Sets the timeout for API requests.
    /// </summary>
    /// <param name="timeout">The timeout duration for requests</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timeout is zero or negative</exception>
    public OpenAIServiceConfiguration UseTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), 
                "Timeout must be greater than zero.");
            
        Options.RequestTimeout = timeout;
        return this;
    }
    
    /// <summary>
    /// Configures retry behavior for failed requests.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts (0 to disable)</param>
    /// <param name="retryDelay">Base delay between retries (uses exponential backoff)</param>
    /// <returns>The configuration builder for chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxRetries is negative or retryDelay is negative</exception>
    public OpenAIServiceConfiguration UseRetryPolicy(int maxRetries, TimeSpan? retryDelay = null)
    {
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), 
                "MaxRetries cannot be negative.");
        if (retryDelay.HasValue && retryDelay.Value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retryDelay), 
                "RetryDelay cannot be negative.");
            
        Options.MaxRetries = maxRetries;
        Options.RetryDelay = retryDelay;
        return this;
    }
    
    /// <summary>
    /// Configures Azure OpenAI service with the specified resource endpoint.
    /// </summary>
    /// <param name="resourceName">The Azure OpenAI resource name</param>
    /// <param name="apiKey">The Azure OpenAI API key</param>
    /// <param name="deploymentName">The deployment/model name</param>
    /// <returns>The configuration builder for chaining</returns>
    public OpenAIServiceConfiguration UseAzureOpenAI(
        string resourceName, 
        string apiKey, 
        string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(deploymentName))
            throw new ArgumentException("Deployment name cannot be null or empty.", nameof(deploymentName));
            
        Options.BaseUrl = $"https://{resourceName}.openai.azure.com/";
        Options.ApiKey = apiKey;
        Options.DefaultModel = deploymentName;
        return this;
    }
    
    /// <summary>
    /// Configures a local LLM server (e.g., Ollama, LocalAI, LM Studio).
    /// </summary>
    /// <param name="endpoint">The local server endpoint (e.g., "http://localhost:11434/v1")</param>
    /// <param name="model">The model name</param>
    /// <param name="apiKey">Optional API key (default: "not-needed")</param>
    /// <returns>The configuration builder for chaining</returns>
    public OpenAIServiceConfiguration UseLocalLLM(
        string endpoint, 
        string model, 
        string apiKey = "not-needed")
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be null or empty.", nameof(model));
            
        Options.BaseUrl = endpoint;
        Options.ApiKey = apiKey;
        Options.DefaultModel = model;
        return this;
    }
}


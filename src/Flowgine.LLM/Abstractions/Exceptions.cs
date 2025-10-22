namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Base exception for all Flowgine LLM-related errors.
/// </summary>
public class FlowgineLLMException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlowgineLLMException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public FlowgineLLMException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FlowgineLLMException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public FlowgineLLMException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an API key is missing or invalid.
/// </summary>
public class ApiKeyMissingException : FlowgineLLMException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyMissingException"/> class.
    /// </summary>
    public ApiKeyMissingException() 
        : base("API key is missing or invalid. Please configure a valid API key.") { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyMissingException"/> class.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    public ApiKeyMissingException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a rate limit is exceeded.
/// </summary>
public class RateLimitException : FlowgineLLMException
{
    /// <summary>
    /// Gets the time to wait before retrying, if provided by the API.
    /// </summary>
    public TimeSpan? RetryAfter { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="retryAfter">Optional time to wait before retrying.</param>
    public RateLimitException(TimeSpan? retryAfter = null)
        : base(retryAfter.HasValue 
            ? $"Rate limit exceeded. Retry after {retryAfter.Value.TotalSeconds:F0} seconds."
            : "Rate limit exceeded. Please try again later.")
    {
        RetryAfter = retryAfter;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitException"/> class.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    /// <param name="retryAfter">Optional time to wait before retrying.</param>
    public RateLimitException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Exception thrown when the API returns an invalid or unexpected response.
/// </summary>
public class InvalidResponseException : FlowgineLLMException
{
    /// <summary>
    /// Gets the raw response from the API, if available.
    /// </summary>
    public string? RawResponse { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="rawResponse">Optional raw response from the API.</param>
    public InvalidResponseException(string message, string? rawResponse = null)
        : base(message)
    {
        RawResponse = rawResponse;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidResponseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="rawResponse">Optional raw response from the API.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidResponseException(string message, string? rawResponse, Exception innerException)
        : base(message, innerException)
    {
        RawResponse = rawResponse;
    }
}

/// <summary>
/// Exception thrown when request validation fails.
/// </summary>
public class ValidationException : FlowgineLLMException
{
    /// <summary>
    /// Gets the name of the parameter that failed validation.
    /// </summary>
    public string? ParameterName { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="parameterName">The name of the parameter that failed validation.</param>
    public ValidationException(string message, string? parameterName = null)
        : base(message)
    {
        ParameterName = parameterName;
    }
}


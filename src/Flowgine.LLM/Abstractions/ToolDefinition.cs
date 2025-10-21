namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Defines a function/tool that the LLM can call.
/// </summary>
public sealed record ToolDefinition
{
    /// <summary>
    /// The name of the function (e.g., "get_weather", "search_database").
    /// Must be unique within the request.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Description of what the function does. 
    /// The LLM uses this to decide when to call the function.
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// JSON Schema describing the function parameters.
    /// Can be a string (raw JSON schema) or an anonymous object that will be serialized to JSON.
    /// </summary>
    /// <example>
    /// <code>
    /// Parameters = new
    /// {
    ///     type = "object",
    ///     properties = new
    ///     {
    ///         location = new { type = "string", description = "City name" },
    ///         unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
    ///     },
    ///     required = new[] { "location" }
    /// }
    /// </code>
    /// </example>
    public object? Parameters { get; init; }
    
    /// <summary>
    /// Whether the function parameters should be strictly validated against the schema.
    /// Supported by OpenAI models with structured outputs.
    /// </summary>
    public bool? Strict { get; init; }
}

/// <summary>
/// Specifies how the LLM should use tools/functions.
/// </summary>
public enum ToolChoice
{
    /// <summary>
    /// LLM automatically decides whether to call tools based on the conversation context (default).
    /// </summary>
    Auto,
    
    /// <summary>
    /// LLM must call at least one tool before responding.
    /// </summary>
    Required,
    
    /// <summary>
    /// LLM must not call any tools and should respond directly.
    /// </summary>
    None,
    
    /// <summary>
    /// LLM must call a specific tool identified by name.
    /// Use <see cref="ToolConfiguration.ChoiceName"/> to specify which tool.
    /// </summary>
    Specific
}

/// <summary>
/// Configuration for tool/function calling in chat requests.
/// </summary>
public sealed record ToolConfiguration
{
    /// <summary>
    /// List of available tools/functions that the LLM can call.
    /// </summary>
    public required IReadOnlyList<ToolDefinition> Tools { get; init; }
    
    /// <summary>
    /// Controls how the LLM should choose tools.
    /// Defaults to <see cref="ToolChoice.Auto"/>.
    /// </summary>
    public ToolChoice Choice { get; init; } = ToolChoice.Auto;
    
    /// <summary>
    /// Name of the specific tool to use when <see cref="Choice"/> is <see cref="ToolChoice.Specific"/>.
    /// Must match one of the tool names in <see cref="Tools"/>.
    /// </summary>
    public string? ChoiceName { get; init; }
}


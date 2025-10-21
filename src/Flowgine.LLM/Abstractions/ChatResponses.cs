namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Token usage statistics for a completion.
/// </summary>
/// <param name="PromptTokens">Number of tokens in the input prompt.</param>
/// <param name="CompletionTokens">Number of tokens in the generated completion.</param>
/// <param name="TotalTokens">Total tokens used (prompt + completion).</param>
public sealed record UsageInfo(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);

/// <summary>
/// Indicates why the model stopped generating tokens.
/// </summary>
public enum FinishReason
{
    /// <summary>
    /// Unknown or not specified by the provider.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Natural stop point (model decided to stop).
    /// </summary>
    Stop,
    
    /// <summary>
    /// Reached the maximum token limit.
    /// </summary>
    Length,
    
    /// <summary>
    /// Model wants to call a tool/function.
    /// </summary>
    ToolCalls,
    
    /// <summary>
    /// Content was filtered by content moderation.
    /// </summary>
    ContentFilter,
    
    /// <summary>
    /// Stopped due to encountering a stop sequence.
    /// </summary>
    StopSequence
}

/// <summary>
/// Represents a tool/function call requested by the LLM.
/// </summary>
/// <param name="Name">The name of the tool to call.</param>
/// <param name="ArgumentsJson">The arguments as a JSON string.</param>
/// <param name="Id">Optional unique identifier for this tool call.</param>
public sealed record ToolCall(string Name, string ArgumentsJson, string? Id = null);

/// <summary>
/// Represents a completed chat completion response from an LLM.
/// </summary>
/// <param name="Message">The assistant's response message. May be empty if only tool calls are present.</param>
/// <param name="ToolCalls">List of tool calls requested by the LLM.</param>
/// <param name="Model">The model identifier that generated this response.</param>
/// <param name="FinishReason">Why the model stopped generating.</param>
/// <param name="Usage">Token usage statistics. May be null if provider doesn't report usage.</param>
/// <param name="RawJson">Optional raw JSON response from the provider for debugging.</param>
public sealed record ChatCompletion(
    ChatMessage Message, 
    IReadOnlyList<ToolCall> ToolCalls, 
    string? Model, 
    FinishReason FinishReason = FinishReason.Unknown,
    UsageInfo? Usage = null,
    string? RawJson = null);

/// <summary>
/// Base class for events emitted during streaming chat completion.
/// </summary>
public abstract record ChatStreamEvent;

/// <summary>
/// Represents a chunk of text tokens received during streaming.
/// </summary>
/// <param name="Text">The text content of this chunk.</param>
public sealed record TokenChunk(string Text) : ChatStreamEvent;

/// <summary>
/// Represents an incremental update to a tool call during streaming.
/// </summary>
/// <param name="Name">The name of the tool being called.</param>
/// <param name="DeltaJson">The incremental JSON arguments for this tool call.</param>
/// <param name="Id">The unique identifier for this tool call.</param>
public sealed record ToolCallDelta(string Name, string DeltaJson, string Id) : ChatStreamEvent;

/// <summary>
/// Indicates that the streaming chat completion has finished.
/// </summary>
/// <param name="Completion">The final aggregated completion result.</param>
public sealed record Completed(ChatCompletion Completion) : ChatStreamEvent;


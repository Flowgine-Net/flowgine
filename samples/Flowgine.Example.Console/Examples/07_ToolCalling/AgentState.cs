using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._07_ToolCalling;

/// <summary>
/// State for the tool calling agent
/// </summary>
public sealed record AgentState
{
    /// <summary>
    /// Conversation history
    /// </summary>
    public required List<ChatMessage> Messages { get; init; }
    
    /// <summary>
    /// Final response from the assistant
    /// </summary>
    public string? FinalResponse { get; init; }
}


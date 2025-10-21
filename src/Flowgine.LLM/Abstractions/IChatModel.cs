namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Represents a chat-based language model capable of generating responses.
/// </summary>
public interface IChatModel
{
    /// <summary>
    /// Generates a complete chat response for the given request.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that resolves to the completed chat response.</returns>
    Task<ChatCompletion> GenerateAsync(ChatRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Streams chat response tokens as they are generated.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An async enumerable of streaming events.</returns>
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, CancellationToken ct = default);
}


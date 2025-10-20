namespace Flowgine.LLM.Abstractions;

public interface IChatModel
{
    Task<ChatCompletion> GenerateAsync(ChatRequest request, CancellationToken ct = default);
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(ChatRequest request, CancellationToken ct = default);
}


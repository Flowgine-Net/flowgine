namespace Flowgine.LLM.Abstractions;

public sealed record ChatRequest(
    IReadOnlyList<ChatMessage> Messages,
    float? Temperature = null,
    int? MaxTokens = null,
    IReadOnlyDictionary<string, object?>? Tools = null);
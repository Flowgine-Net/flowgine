namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Represents a chat completion request.
/// </summary>
/// <param name="Messages">The list of messages in the conversation.</param>
/// <param name="Temperature">Controls randomness (0.0 = deterministic, higher = more random). Range varies by provider: OpenAI 0-2, Anthropic 0-1.</param>
/// <param name="MaxTokens">Maximum number of tokens to generate in the response.</param>
/// <param name="TopP">Nucleus sampling threshold (0.0-1.0). Alternative to temperature. Not all providers support both simultaneously.</param>
/// <param name="TopK">Limits token selection to top K candidates. Provider-specific (Anthropic, Google). Ignored by providers that don't support it.</param>
/// <param name="FrequencyPenalty">Penalizes tokens based on their frequency (0.0-2.0). Reduces repetition. OpenAI/Azure only.</param>
/// <param name="PresencePenalty">Penalizes tokens that have appeared (0.0-2.0). Encourages topic diversity. OpenAI/Azure only.</param>
/// <param name="Stop">Stop sequences that will halt generation when encountered.</param>
/// <param name="Tools">Configuration for function/tool calling.</param>
/// <param name="User">Optional user identifier for tracking and abuse detection.</param>
/// <param name="Seed">Optional seed for deterministic sampling. Not all providers support this.</param>
public sealed record ChatRequest(
    IReadOnlyList<ChatMessage> Messages,
    float? Temperature = null,
    int? MaxTokens = null,
    float? TopP = null,
    int? TopK = null,
    float? FrequencyPenalty = null,
    float? PresencePenalty = null,
    IReadOnlyList<string>? Stop = null,
    ToolConfiguration? Tools = null,
    string? User = null,
    int? Seed = null);


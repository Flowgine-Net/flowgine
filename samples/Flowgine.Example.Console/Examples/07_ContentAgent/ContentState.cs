using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._07_ContentAgent;

public class ContentState
{
    public List<ChatMessage> Messages { get; set; } = new();
    public string BrandVoice { get; set; } = string.Empty;
    public string TopicContent { get; set; } = string.Empty;
}
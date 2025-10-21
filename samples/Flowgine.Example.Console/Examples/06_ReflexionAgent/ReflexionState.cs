using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._06_ReflexionAgent;

public class ReflexionState
{
    public List<ChatMessage> Messages { get; set; } = new();
}
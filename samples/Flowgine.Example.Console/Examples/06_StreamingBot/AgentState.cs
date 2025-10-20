using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._06_StreamingBot;

/// <summary>
/// State for streaming bot example
/// </summary>
public class AgentState
{
    public required string Prompt { get; set; }
    public string StreamedResponse { get; set; } = "";
}



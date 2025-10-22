namespace Flowgine.Example.Console.Examples._08_PromptTemplates;

public sealed record AgentState
{
    public required string Topic { get; init; }
    public required string Style { get; init; }
    public string? GeneratedContent { get; init; }
}


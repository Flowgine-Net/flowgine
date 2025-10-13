namespace Flowgine.LLM.Abstractions;

public enum ChatRole { System, User, Assistant, Tool }
public abstract record ChatContent;
public sealed record TextContent(string Text) : ChatContent;

/// <summary>
/// Jedna zpráva nemusí být jen jeden text.
/// Parts umožňuje mít v jedné message více obsahových segmentů a typů v přesném pořadí.
/// </summary>
public sealed record ChatMessage(ChatRole Role, IReadOnlyList<ChatContent> Parts)
{
    public static ChatMessage System(string t)   => new(ChatRole.System, [new TextContent(t)]);
    public static ChatMessage User(string t)     => new(ChatRole.User, [new TextContent(t)]);
    public static ChatMessage Assistant(string t)=> new(ChatRole.Assistant, [new TextContent(t)]);
}
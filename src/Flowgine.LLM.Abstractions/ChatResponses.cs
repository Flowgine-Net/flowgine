namespace Flowgine.LLM.Abstractions;

public sealed record ToolCall(string Name, string ArgumentsJson, string? Id = null);
public sealed record ChatCompletion(ChatMessage Message, IReadOnlyList<ToolCall> ToolCalls, string? Model, string? RawJson = null);

public abstract record ChatStreamEvent;
public sealed record TokenChunk(string Text) : ChatStreamEvent;
public sealed record ToolCallDelta(string Name, string DeltaJson, string Id) : ChatStreamEvent;
public sealed record Completed(ChatCompletion Completion) : ChatStreamEvent;
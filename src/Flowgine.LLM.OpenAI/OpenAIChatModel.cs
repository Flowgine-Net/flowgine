using System.Runtime.CompilerServices;

using Flowgine.LLM.Abstractions;

using OpenAI.Chat;
using ChatCompletion = OpenAI.Chat.ChatCompletion;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace Flowgine.LLM.OpenAI;

public sealed class OpenAIChatModel : IChatModel
{
    private readonly ChatClient _client;
    
    public OpenAIChatModel(ChatClient client)
    {
        _client = client;
    }

    public async Task<Flowgine.LLM.Abstractions.ChatCompletion> GenerateAsync(
        ChatRequest request, CancellationToken ct = default)
    {
        var input = ToOpenAIInput(request.Messages);
        var settings = BuildCompletionOptions(request);
        
        // TODO: mapování Tools -> settings.Tools (a tool choice) pokud SDK podporuje
        
        ChatCompletion result= await _client.CompleteChatAsync(input, settings, ct);

        return FromOpenAI(result);
    }

    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var input = ToOpenAIInput(request.Messages);
        var settings = BuildCompletionOptions(request);

        await foreach (var ev in _client.CompleteChatStreamingAsync(input, settings, ct))
        {
            /*if (ev is ChatCompletionChunk chunk)
            {
                var deltaText = chunk.ContentDelta; // ilustrativně – uprav dle skutečného typu
                if (!string.IsNullOrEmpty(deltaText))
                    yield return new TokenChunk(deltaText);
            }
            else if (ev is ChatToolCallDelta toolDelta)
            {
                yield return new ToolCallDelta(toolDelta.Name, toolDelta.ArgumentsJson, toolDelta.Id);
            }
            else if (ev is ChatCompletion final)
            {
                yield return new Completed(FromOpenAI(final));
            }*/
            yield return new Completed(null);
        }
    }

    // ---------- Type mapping ----------
    private ChatCompletionOptions BuildCompletionOptions(ChatRequest request)
    {
        var options = new ChatCompletionOptions();
        
        ApplyIfHasValue(request.Temperature, v => options.Temperature = v);
        ApplyIfHasValue(request.MaxTokens,   v => options.MaxOutputTokenCount = v);

        return options;
    }
    
    // Reusable helper that assigns only when the request provided a value.
    // No instance defaults are considered.
    private static void ApplyIfHasValue<T>(T? reqVal, Action<T> assign) where T : struct
    {
        if (reqVal.HasValue)
        {
            assign(reqVal.Value);
        }
        // else: omit -> server default on OpenAI side
    }

    private static IEnumerable<ChatMessage> ToOpenAIInput(IReadOnlyList<Flowgine.LLM.Abstractions.ChatMessage> msgs)
    {
        foreach (var m in msgs)
        {
            var text = string.Join("", m.Parts.OfType<TextContent>().Select(p => p.Text));
            yield return m.Role switch
            {
                ChatRole.System    => ChatMessage.CreateSystemMessage(text),
                ChatRole.User      => ChatMessage.CreateUserMessage(text),
                ChatRole.Assistant => ChatMessage.CreateAssistantMessage(text),
                _ => ChatMessage.CreateUserMessage(text),
            };
        }
    }
    
    private static Flowgine.LLM.Abstractions.ChatCompletion FromOpenAI(ChatCompletion completion)
    {
        var txt = completion.Content?.FirstOrDefault()?.Text ?? "";
        var message = new Flowgine.LLM.Abstractions.ChatMessage(
            ChatRole.Assistant, [new TextContent(txt)]);

        var tools = completion.ToolCalls?.Select(tc =>
            new ToolCall(tc.FunctionName, tc.FunctionArguments.ToString(), tc.Id)).ToArray() ?? [];

        return new Flowgine.LLM.Abstractions.ChatCompletion(message, tools, completion.Model, completion.Content.ToString()); //Raw content?
    }
}
using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._05_ReflectionAgent;

public class ReflectionNode : AsyncNode<AgentState>
{
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();
        
        var messages = new List<ChatMessage>
        {
            ChatMessage.System("You are a viral twitter influencer grading a tweet. " +
                               "Generate critique and recommendations for the user's tweet. " +
                               "Always provide detailed recommendations, including requests for length, virality, style, etc.")
        };
        messages.AddRange(state.Messages);
        
        var req = new ChatRequest(messages);
        var completion = await llm.GenerateAsync(req, ct);
        var text = (completion.Message.Parts[0] as TextContent)?.Text ?? "";
        
        var updatedMessages = new List<ChatMessage>(state.Messages)
        {
            ChatMessage.User(text)
        };
        
        return Update.Of<AgentState>()
            .Set(s => s.Messages, updatedMessages);
    }
}
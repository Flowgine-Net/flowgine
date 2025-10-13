using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._05_ReflectionAgent;

public class GenerationNode : AsyncNode<AgentState>
{
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();
        
        var messages = new List<ChatMessage>
        {
            ChatMessage.System("You are a twitter techie influencer assistant tasked " +
                               "with writing excellent twitter posts. Generate the best twitter post possible " +
                               "for the user's request. If the user provides critique, respond with a revised version of your previous attempts.")
        };
        messages.AddRange(state.Messages);

        var req = new ChatRequest(messages);
        var completion = await llm.GenerateAsync(req, ct);
        var text = (completion.Message.Parts[0] as TextContent)?.Text ?? "";
        
        state.Messages.Add(ChatMessage.Assistant(text));
        
        return Update.Of<AgentState>()
            .Set(s => s.Messages, state.Messages);
    }
}
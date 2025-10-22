using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._04_SimpleBot;

public class AskNode: AsyncNode<AgentState>
{
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();
        
        var req = new ChatRequest([
            ChatMessage.System("Be concise."),
            ChatMessage.User(state.Prompt)
        ]);

        var completion = await llm.GenerateAsync(req, ct);
        var text = (completion.Message.Parts[0] as TextContent)?.Text ?? "";
        
        return Update.Of<AgentState>()
            .Set(x => x.LastAnswer, text);
    }
}
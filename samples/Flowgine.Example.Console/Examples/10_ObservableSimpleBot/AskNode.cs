using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;
using Flowgine.Observability;

namespace Flowgine.Example.Console.Examples._10_ObservableSimpleBot;

public class AskNode : AsyncNode<AgentState>, IObservableNode
{
    public string ObservationType => "agent";
    
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();

        // Get observability provider and wrap LLM if available
        var obsProvider = runtime.Get<IObservabilityProvider>();
        if (obsProvider != null)
            llm = llm.WithObservability(obsProvider);

        var req = new ChatRequest([
            ChatMessage.System("Be concise."),
            ChatMessage.User(state.Prompt)
        ]);

        var completion = await llm.GenerateAsync(req, ct);
        var text = (completion.Message.Parts[0] as TextContent)?.Text ?? "";

        System.Console.WriteLine($"Assistant: {text}");

        return Update.Of<AgentState>()
            .Set(x => x.LastAnswer, text);
    }
}


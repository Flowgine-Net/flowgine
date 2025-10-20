using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._06_StreamingBot;

/// <summary>
/// Node that demonstrates streaming LLM responses token-by-token
/// </summary>
public class StreamingNode : AsyncNode<AgentState>
{
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();
        
        var req = new ChatRequest([
            ChatMessage.System("You are a helpful assistant. Be descriptive and engaging in your responses."),
            ChatMessage.User(state.Prompt)
        ]);

        var fullResponse = new System.Text.StringBuilder();
        
        System.Console.Write("\n🤖 Assistant: ");
        
        await foreach (var streamEvent in llm.StreamAsync(req, ct))
        {
            switch (streamEvent)
            {
                case TokenChunk chunk:
                    // Print each token as it arrives
                    System.Console.Write(chunk.Text);
                    fullResponse.Append(chunk.Text);
                    
                    // Add small delay to make streaming visible
                    await Task.Delay(10, ct);
                    break;
                    
                case ToolCallDelta toolDelta:
                    System.Console.WriteLine($"\n[Tool call: {toolDelta.Name}]");
                    break;
                    
                case Completed completed:
                    System.Console.WriteLine("\n");
                    break;
            }
        }
        
        return Update.Of<AgentState>()
            .Set(x => x.StreamedResponse, fullResponse.ToString());
    }
}



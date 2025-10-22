using Flowgine.Example.Console.Shared;
using Flowgine.Core;

namespace Flowgine.Example.Console.Examples._06_StreamingBot;

public class Run : IExample
{
    public string Id => "06-streaming-bot";
    public string Title => "StreamingBot - Real-time token-by-token LLM responses";

    public async Task RunAsync(CancellationToken ct = default)
    {
        System.Console.WriteLine("=== Streaming Bot Example ===");
        System.Console.WriteLine("This example demonstrates real-time streaming of LLM responses.\n");
        
        var flow = new Flowgine<AgentState>();
        var streamingNode = flow.AddNode(new StreamingNode());
        
        flow.SetEntryPoint(streamingNode)
            .SetFinishPoint(streamingNode);

        var compiledFlow = flow.Compile();
        
        // Test with a prompt that will generate a longer response
        var state = new AgentState 
        { 
            Prompt = "Explain in 3 sentences what makes a good software architecture." 
        };
        
        System.Console.WriteLine($"👤 User: {state.Prompt}");
        
        var final = await compiledFlow.RunToCompletionAsync(state, Program.Services!, ct);
        
        System.Console.WriteLine($"\n✅ Streaming completed!");
        System.Console.WriteLine($"📊 Total characters received: {final.StreamedResponse.Length}");
    }
}


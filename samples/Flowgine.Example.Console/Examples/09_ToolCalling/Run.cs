using Flowgine.Example.Console.Shared;
using Flowgine.Core;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._09_ToolCalling;

public sealed class Run : IExample
{
    public string Id => "09-tool-calling";
    public string Title => "Tool Calling - AI Assistant with Weather, Time & Calculator";

    public async Task RunAsync(CancellationToken ct = default)
    {
        System.Console.WriteLine("=== Tool Calling Example ===");
        System.Console.WriteLine("This example demonstrates function/tool calling.");
        System.Console.WriteLine("The AI can use tools to get real information:\n");
        System.Console.WriteLine("  🕐 get_current_time - Get current time in any timezone");
        System.Console.WriteLine("  ☀️  get_weather - Get weather for any city");
        System.Console.WriteLine("  🧮 calculate - Perform calculations\n");
        
        var flow = new Flowgine<AgentState>();
        var assistantNode = flow.AddNode(new AssistantNode());
        
        flow.SetEntryPoint(assistantNode)
            .SetFinishPoint(assistantNode);

        var compiledFlow = flow.Compile();
        
        // Test different queries
        var queries = new[]
        {
            "What time is it in Prague?",
            "What's the weather like in London?",
            "Can you calculate 42 * 137 + 95?"
        };
        
        foreach (var query in queries)
        {
            System.Console.WriteLine("\n" + new string('=', 80));
            System.Console.WriteLine($"👤 User: {query}");
            System.Console.WriteLine(new string('=', 80));
            
            var state = new AgentState
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.System("You are a helpful assistant with access to tools for getting time, weather, and performing calculations. Use these tools when appropriate."),
                    ChatMessage.User(query)
                }
            };
            
            var result = await compiledFlow.RunToCompletionAsync(state, Program.Services!, ct);
            
            // Small delay between queries for readability
            await Task.Delay(1000, ct);
        }
        
        System.Console.WriteLine("\n" + new string('=', 80));
        System.Console.WriteLine("✅ Tool calling demonstration completed!");
        System.Console.WriteLine("\nKey takeaways:");
        System.Console.WriteLine("  • LLM automatically decides when to use tools");
        System.Console.WriteLine("  • Tools are called with structured parameters");
        System.Console.WriteLine("  • Results are sent back to LLM for final response");
        System.Console.WriteLine("  • The flow is fully type-safe with ToolConfiguration");
    }
}


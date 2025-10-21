using Flowgine.Core;
using Flowgine.Example.Console.Shared;

namespace Flowgine.Example.Console.Examples._08_PromptTemplates;

public sealed class Run : IExample
{
    public string Id => "08-prompt-templates";
    public string Title => "Prompt Templates - Reusable Prompt Construction";

    public async Task RunAsync(CancellationToken ct = default)
    {
        System.Console.WriteLine("=== Prompt Template Example ===\n");
        System.Console.WriteLine("Demonstrating reusable prompt templates with variable substitution.\n");

        var initialState = new AgentState
        {
            Topic = "artificial intelligence",
            Style = "educational and engaging"
        };

        var flow = new Flowgine<AgentState>();
        var generator = flow.AddNode(new ContentGeneratorNode());
        
        flow.SetEntryPoint(generator)
            .SetFinishPoint(generator);
        
        var compiled = flow.Compile();
        
        var result = await compiled.RunToCompletionAsync(initialState, Program.Services, ct);

        System.Console.WriteLine($"✅ Final state:");
        System.Console.WriteLine($"   Topic: {result.Topic}");
        System.Console.WriteLine($"   Style: {result.Style}");
        System.Console.WriteLine($"   Content length: {result.GeneratedContent?.Length ?? 0} characters");
        
        System.Console.WriteLine("\n💡 Template variables used: topic, style");
        System.Console.WriteLine("💡 This template can be reused for any topic and style combination!");
    }
}


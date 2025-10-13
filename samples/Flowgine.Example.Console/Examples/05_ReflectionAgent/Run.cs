using Flowgine.Example.Console.Shared;
using Flowgine.Core;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._05_ReflectionAgent;

public class Run : IExample
{
    public string Id => "05-reflection-agent";
    public string Title => "Reflection agent - AI system that iteratively improves its output by critiquing and refining its own work";

    public async Task RunAsync(CancellationToken ct = default)
    {
        var flow = new Flowgine<AgentState>()
            .AddNode(new ReflectionNode())
            .AddNode(new GenerationNode())
            .SetEntryPoint(nameof(GenerationNode))
            .SetFinishPoint(nameof(ReflectionNode))
            .AddEdge(nameof(GenerationNode), nameof(ReflectionNode));
        
        var compiledFlow = flow.Compile();
        
        var state = new AgentState()
        {
            Messages = [
                ChatMessage.User("Make this tweet better: @LangChainAI\n — newly Tool Calling feature is seriously underrated." +
                                 "After a long wait, it's  here- making the implementation of agents across different models with function calling - super easy." +
                                 "Made a video covering their newest blog post")
            ]
        };
        
        var final = await compiledFlow.RunToCompletionAsync(state, Guid.NewGuid(), Program.Services!, ct);
    }
}
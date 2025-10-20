using Flowgine.Core;
using Flowgine.Example.Console.Shared;

namespace Flowgine.Example.Console.Examples._04_SimpleBot;

public class Run: IExample
{
    public string Id => "04-simple-bot";
    public string Title => "SimpleBot - How to integrate LLMs in our flows";
    public async Task RunAsync(CancellationToken ct = default)
    {
        var flow = new Flowgine<AgentState>();
        var ask = flow.AddNode(new AskNode());
        
        flow.SetEntryPoint(ask)
            .SetFinishPoint(ask);
        
        var compiledFlow = flow.Compile();
        
        var state = new AgentState { Prompt = "Write a 3-word greeting." };
        
        var final = await compiledFlow.RunToCompletionAsync(state, Program.Services!, ct);
    }
}

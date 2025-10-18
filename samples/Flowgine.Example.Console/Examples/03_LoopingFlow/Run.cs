using Flowgine.Abstractions;
using Flowgine.Core;
using Flowgine.Example.Console.Shared;

namespace Flowgine.Example.Console.Examples._03_LoopingFlow;

public class Run : IExample
{
    public string Id => "03-looping";
    public string Title => "Looping flow with router node.";

    public async Task RunAsync(CancellationToken ct = default)
    {
        var flow = new Flowgine<AgentState>();
        
        var greeting = flow.AddNode(new GreetingNode());
        var random = flow.AddNode(new RandomNode());
        var router = flow.AddNode(new RouterNode(random));
        
        flow.SetEntryPoint(greeting)
            .AddEdge(greeting, random)
            .AddEdge(random, router);

        var compiled = flow.Compile();

        var state = new AgentState() { Name = "Flowgine", Counter = 0 };

        var final = await compiled.RunToCompletionAsync(state, Guid.NewGuid(),null, ct);

        foreach(var number in final.Numbers)
        {
            System.Console.WriteLine($"Generated number: {number}");
        } 
    }
}

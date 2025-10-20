using Flowgine.Abstractions;
using Flowgine.Core;
using Flowgine.Example.Console.Shared;

namespace Flowgine.Example.Console.Examples._02_ConditionalFlow;

public class Run : IExample
{
    public string Id => "02-conditional";
    public string Title => "Conditional flow with router node.";

    public async Task RunAsync(CancellationToken ct = default)
    {
        var flow = new Flowgine<AgentState>();
        
        var adder = flow.AddNode(new AdderNode());
        var subtractor = flow.AddNode(new SubtractorNode());
        var router = flow.AddNode(new RouterNode(adder, subtractor));
        
        flow.SetEntryPoint(router)
            .SetFinishPoint(adder)
            .SetFinishPoint(subtractor);

        var compiled = flow.Compile();

        var state = new AgentState { Number1 = 5, Number2 = 2, Operation = "-" };

        await foreach (var ev in compiled.RunAsync(state, ct))
        {
            switch (ev)
            {
                case NodeStarted<AgentState> s: System.Console.WriteLine($"→ {s.NodeName}"); break;
                case NodeCompleted<AgentState> c: System.Console.WriteLine($"✓ X={c.State.Result}"); break;
            }
        }
    }
}

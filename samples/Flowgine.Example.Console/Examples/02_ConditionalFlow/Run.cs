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
        var flow = new Flowgine<AgentState>()
            .AddNode(new RouterNode())
            .AddNode(new AdderNode())
            .AddNode(new SubtractorNode())
            .SetEntryPoint(nameof(RouterNode))
            .SetFinishPoint(nameof(AdderNode))
            .SetFinishPoint(nameof(SubtractorNode));

        var compiled = flow.Compile();

        var state = new AgentState { Number1 = 5, Number2 = 2, Operation = "-" };

        await foreach (var ev in compiled.RunAsync(state, Guid.NewGuid(), ct))
        {
            switch (ev)
            {
                case NodeStarted<AgentState> s: System.Console.WriteLine($"→ {s.NodeName}"); break;
                case NodeCompleted<AgentState> c: System.Console.WriteLine($"✓ X={c.State.Result}"); break;
            }
        }
    }
}

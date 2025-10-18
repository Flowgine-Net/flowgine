using Flowgine.Abstractions;
using Flowgine.Core;
using Flowgine.Example.Console.Shared;

namespace Flowgine.Example.Console.Examples._01_Basics;

public sealed class Run : IExample
{
    public string Id => "01-basics";
    public string Title => "Basics: typed partial update + START/END";

    public async Task RunAsync(CancellationToken ct = default)
    {
        var flow = new Flowgine<AgentState>();
        var map = flow.AddNode(new MapNode());
        
        flow.SetEntryPoint(map)
            .SetFinishPoint(map);

        var compiled = flow.Compile();

        var state = new AgentState { X = 0.5m };

        await foreach (var ev in compiled.RunAsync(state, Guid.NewGuid(), null, ct))
        {
            switch (ev)
            {
                case NodeStarted<AgentState> s: System.Console.WriteLine($"→ {s.NodeName}"); break;
                case NodeCompleted<AgentState> c: System.Console.WriteLine($"✓ X={c.State.X}"); break;
            }
        }
    }
}
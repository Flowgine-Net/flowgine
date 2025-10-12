// See https://aka.ms/new-console-template for more information
using Flowgine.Abstractions;
using Flowgine.Core;

Console.WriteLine("Hello, Flogine!");

var entryNode = new MapNode();

var flow = new Flowgine<TestState>()
    .AddNode(entryNode)
    .SetEntryPoint(entryNode.Name);

var compiled = flow.Compile();

await foreach (var ev in compiled.RunAsync(new TestState(10m), Guid.NewGuid()))
{
    switch (ev)
    {
        case NodeStarted<TestState> s:
            Console.WriteLine($"→ START {s.NodeName}");
            break;

        case NodeCompleted<TestState> c:
            Console.WriteLine($"✓ DONE  {c.NodeName}: Stage={c.State.X}");
            break;

        case BranchTaken<TestState> b:
            Console.WriteLine($"↪ BRANCH from {b.From} -> [{string.Join(", ", b.To)}]");
            break;

        case Interrupted<TestState> i:
            Console.WriteLine($"! INTERRUPTED: {i.Reason}");
            break;
    }
}

public record TestState(decimal X);

public class MapNode : INode<TestState>
{
    public string Name => "MapNode";

    public object? Invoke(TestState state, CancellationToken ct = default)
    {
        var next = state.X * (1 - state.X);
        
        // Silně typový partial update:
        return Update.Of<TestState>()
            .Set(s => s.X, next);
    }
}
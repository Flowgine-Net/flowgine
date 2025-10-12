using Flowgine.Example.Console.Shared;

// Reister new examples here
var examples = new IExample[]
{
    new Flowgine.Example.Console.Examples._01_Basics.Run(),
    new Flowgine.Example.Console.Examples._02_ConditionalFlow.Run(),
    new Flowgine.Example.Console.Examples._03_LoopingFlow.Run(),
};

var map = examples.ToDictionary(e => e.Id, e => e, StringComparer.OrdinalIgnoreCase);

var exId = Cli.GetArg(args, "--example") ?? "03-looping";

if (!map.TryGetValue(exId, out var example))
{
    System.Console.WriteLine("Available examples:");
    foreach (var e in examples.OrderBy(e => e.Id))
        System.Console.WriteLine($"  {e.Id,-14} {e.Title}");
    System.Environment.Exit(1);
}

System.Console.WriteLine($"Running: {example.Id} - {example.Title}");
await example.RunAsync();
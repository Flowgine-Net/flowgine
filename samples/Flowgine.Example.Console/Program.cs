using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Flowgine.Example.Console.Shared;
using Flowgine.LLM.Abstractions;
using Flowgine.LLM.OpenAI;

// Load configurations (JSON + environment variables)
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()  // Can load OPENAI__APIKEY from env variables
    .Build();

var services = new ServiceCollection()
    .Configure<OpenAIChatOptions>(config.GetSection("OpenAI"))
    .AddSingleton<IOpenAIProvider, OpenAIProvider>()
    .BuildServiceProvider();

Program.Services = services;

// Reister new examples here
var examples = new IExample[]
{
    new Flowgine.Example.Console.Examples._01_Basics.Run(),
    new Flowgine.Example.Console.Examples._02_ConditionalFlow.Run(),
    new Flowgine.Example.Console.Examples._03_LoopingFlow.Run(),
    new Flowgine.Example.Console.Examples._04_SimpleBot.Run(),
    new Flowgine.Example.Console.Examples._05_ReflectionAgent.Run(),
    new Flowgine.Example.Console.Examples._08_StreamingBot.Run(),
};

var map = examples.ToDictionary(e => e.Id, e => e, StringComparer.OrdinalIgnoreCase);

var exId = Cli.GetArg(args, "--example") ?? "05-reflection-agent";

if (!map.TryGetValue(exId, out var example))
{
    System.Console.WriteLine("Available examples:");
    foreach (var e in examples.OrderBy(e => e.Id))
        System.Console.WriteLine($"  {e.Id,-14} {e.Title}");
    System.Environment.Exit(1);
}

System.Console.WriteLine($"Running: {example.Id} - {example.Title}");
await example.RunAsync();

public static partial class Program
{
    public static IServiceProvider Services { get; set; }
}
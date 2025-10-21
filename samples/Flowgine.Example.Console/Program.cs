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

// Three ways to register OpenAI services:

// Option 1: Using configuration from appsettings.json
var services = new ServiceCollection()
    .AddOpenAI(config.GetSection("OpenAI"))
    .BuildServiceProvider();

// Option 2: Direct configuration with lambda
// var services = new ServiceCollection()
//     .AddOpenAI(options =>
//     {
//         options.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your-api-key";
//         options.DefaultModel = "gpt-4o-mini";
//         options.DefaultTemperature = 0.7f;
//     })
//     .BuildServiceProvider();

// Option 3: Fluent configuration builder
// var services = new ServiceCollection()
//     .AddOpenAI(cfg => cfg
//         .UseApiKey(Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your-api-key")
//         .UseModel("gpt-4o-mini")
//         .UseTemperature(0.7f))
//     .BuildServiceProvider();

// Azure OpenAI example:
// var services = new ServiceCollection()
//     .AddOpenAI(cfg => cfg
//         .UseAzureOpenAI("your-resource-name", "your-api-key", "gpt-4"))
//     .BuildServiceProvider();

// Local LLM example (Ollama):
// var services = new ServiceCollection()
//     .AddOpenAI(cfg => cfg
//         .UseLocalLLM("http://localhost:11434/v1", "llama2"))
//     .BuildServiceProvider();

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
    new Flowgine.Example.Console.Examples._09_ToolCalling.Run(),
    new Flowgine.Example.Console.Examples._10_PromptTemplates.Run(),
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
using Flowgine.Example.Console.Shared;
using Flowgine.LLM.Abstractions;
using Flowgine.Observability;
using Flowgine.Observability.Langfuse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Flowgine.Example.Console.Examples._09_Observability;

public sealed class Run : IExample
{
    public string Id => "09-observability";
    public string Title => "Observability: Langfuse + OpenTelemetry";

    public async Task RunAsync(CancellationToken ct = default)
    {
        // Load configuration (supports appsettings.json, appsettings.local.json, env)
        /*var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var lf = config.GetSection("Langfuse");
        var publicKey = lf["PublicKey"] ?? Environment.GetEnvironmentVariable("LANGFUSE_PUBLIC_KEY");
        var secretKey = lf["SecretKey"] ?? Environment.GetEnvironmentVariable("LANGFUSE_SECRET_KEY");
        var host = lf["Host"] ?? Environment.GetEnvironmentVariable("LANGFUSE_HOST");

        if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            System.Console.WriteLine("Missing Langfuse configuration. Provide 'Langfuse:PublicKey' and 'Langfuse:SecretKey' in appsettings.local.json or environment variables (LANGFUSE_PUBLIC_KEY / LANGFUSE_SECRET_KEY).");
            return;
        }

        // Configure observability (OTLP + optional console exporter)
        using var obsServices = new ServiceCollection()
            .AddLangfuseObservability(
                publicKey,
                secretKey,
                applicationName: "flowgine-examples",
                langfuseHost: host,
                configure: o => o.EnableConsoleExporter = false) // Set to true for local debugging
            .BuildServiceProvider();*/

        // Force TracerProvider initialization to enable ActivitySource listeners
        _ = Program.Services.GetService<OpenTelemetry.Trace.TracerProvider>();
        
        var provider = Program.Services.GetRequiredService<IObservabilityProvider>();

        // Start trace context for this example
        var runId = Guid.NewGuid();
        var trace = await provider.StartTraceAsync("Flowgine-demo", runId, input: "In one sentence, explain what OpenTelemetry is.", ct);
        var previous = TraceContext.Current;
        TraceContext.Current = trace;
        
        // Resolve a chat model from the main Program service provider
        var openAiProvider = Program.Services.GetRequiredService<IOpenAIProvider>();
        var baseModel = openAiProvider.GetModel();
        var model = baseModel.WithObservability(provider);

        string? text = null;
        try
        {
            var request = new ChatRequest(
                Messages: new[]
                {
                    ChatMessage.System("You are a helpful assistant."),
                    ChatMessage.User("In one sentence, explain what OpenTelemetry is.")
                },
                //Temperature: 0.2f,
                MaxTokens: 12800);

            var completion = await model.GenerateAsync(request, ct);
            text = string.Join("", completion.Message.Parts.OfType<TextContent>().Select(p => p.Text));
            
            System.Console.WriteLine($"Assistant: {text}");
            
            if (completion.Usage != null)
            {
                System.Console.WriteLine($"Tokens: {completion.Usage.PromptTokens} in, {completion.Usage.CompletionTokens} out, {completion.Usage.TotalTokens} total");
            }
        }
        finally
        {
            TraceContext.Current = previous;
            await provider.EndTraceAsync(trace, text, ct);
            
            System.Console.WriteLine();
            System.Console.WriteLine("⏳ Waiting for trace export...");
            await Task.Delay(6000, ct); // Give TracerProvider time to export
            System.Console.WriteLine($"✅ Trace sent to Langfuse.");
        }
    }
}


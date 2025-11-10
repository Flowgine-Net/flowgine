using Flowgine.Core;
using Flowgine.Example.Console.Shared;
using Flowgine.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Flowgine.Example.Console.Examples._10_ObservableSimpleBot;

public class Run : IExample
{
    public string Id => "10-observable-simple-bot";
    public string Title => "Observable Simple Bot - LLM with Observability";
    
    public async Task RunAsync(CancellationToken ct = default)
    {
         // Force TracerProvider initialization to enable ActivitySource listeners
        _ = Program.Services.GetService<OpenTelemetry.Trace.TracerProvider>();
        var obsProvider = Program.Services.GetService<IObservabilityProvider>();
        
        if (obsProvider != null)
        {
            System.Console.WriteLine("🔍 Observability is enabled (Langfuse)");
            
            // Start trace context for this example
            var runId = Guid.NewGuid();

            try
            {
                await RunFlowAsync(obsProvider, ct);
                
                System.Console.WriteLine();
                System.Console.WriteLine("⏳ Waiting for trace export...");
                await Task.Delay(15000, ct); // Give TracerProvider time to export
                System.Console.WriteLine($"✅ Trace sent to Langfuse. Check your dashboard for trace ID: {runId}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"❌ An error occurred: {ex.Message}");
                await Task.Delay(6000, ct); // Give TracerProvider time to export even on error
                System.Console.WriteLine($"✅ Trace sent to Langfuse. Check your dashboard for trace ID: {runId}");
            }
        }
        else
        {
            System.Console.WriteLine("ℹ️ Observability is not configured. Running without observability.");
        }
    }
    
    private async Task RunFlowAsync(IObservabilityProvider observabilityProvider, CancellationToken ct)
    {
        var flow = new Flowgine<AgentState>();
        var ask = flow.AddNode(new AskNode());
        
        flow.SetEntryPoint(ask)
            .SetFinishPoint(ask);
        
        var compiledFlow = flow.Compile(name:"observable-bot");
        var obsFlow = compiledFlow.WithObservability(observabilityProvider);
        
        var state = new AgentState { Prompt = "Write a 3-word greeting." };
        var runId = Guid.NewGuid();
        
        // Use RunToCompletionAsync for simpler usage
        var final = await obsFlow.RunToCompletionAsync(state, Program.Services!, runId, ct);
        
        System.Console.WriteLine();
        System.Console.WriteLine($"✅ Final answer: {final.LastAnswer}");
    }
}


using System.Diagnostics;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Langfuse implementation of <see cref="IObservabilityProvider"/> using OpenTelemetry.
/// Tracks flow execution, node operations, and LLM interactions in Langfuse platform.
/// </summary>
/// <remarks>
/// This provider uses OpenTelemetry Activities to create spans and traces that are exported
/// to Langfuse via OTLP protocol. It follows Langfuse semantic conventions for proper
/// visualization and analysis in the Langfuse UI.
/// This class should be registered as a singleton and will be disposed when the application shuts down.
/// </remarks>
public sealed class LangfuseProvider : IObservabilityProvider, IDisposable
{
    private readonly ActivitySource _activitySource; 
    private readonly string _applicationName;
    private readonly string _environment;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LangfuseProvider"/> class.
    /// </summary>
    /// <param name="options">Configuration options for Langfuse integration.</param>
    public LangfuseProvider(LangfuseOptions options)
    {
        _applicationName = options.ApplicationName;
        _environment = options.Environment ?? "production";
        var version = typeof(LangfuseProvider).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        _activitySource = new ActivitySource(_applicationName, version);
    }
    
    /// <inheritdoc />
    public Task<ITraceContext> StartTraceAsync(string name, Guid runId, string? input = null, CancellationToken ct = default)
    {
        // Create root activity for the entire flow
        var activity = _activitySource.StartActivity( name, ActivityKind.Server);
        
        if (activity != null) 
        { 
            // Langfuse-specific tags
            activity.SetTag("run.id", runId.ToString()); 
            activity.SetTag("gen_ai.application_name", _applicationName); 
            activity.SetTag("gen_ai.environment", _environment); 
            activity.SetTag("framework", "Flowgine"); 
            activity.SetTag("langfuse.trace.name", name);
            
            // Set input if provided
            if (!string.IsNullOrEmpty(input))
            {
                activity.SetTag("langfuse.trace.input", input);
            }
            
            // Important: Set scope for Langfuse
            activity.SetTag("scope", "langfuse.otel.tracing"); 
        } 
        
        return Task.FromResult<ITraceContext>( 
            new LangfuseTraceContext(activity, runId));
    }

    /// <inheritdoc />
    public Task EndTraceAsync(ITraceContext trace, string? output = null, CancellationToken ct = default)
    {
        var langfuseTrace = (LangfuseTraceContext)trace;
        
        // Stop the root activity to trigger export
        if (langfuseTrace.Activity != null)
        {
            // Set output if provided
            if (!string.IsNullOrEmpty(output))
            {
                langfuseTrace.Activity.SetTag("langfuse.trace.output", output);
            }
            
            langfuseTrace.Activity.SetStatus(ActivityStatusCode.Ok);
            langfuseTrace.Activity.Stop();
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<ISpanContext> StartSpanAsync( 
        ITraceContext trace, 
        string nodeName, 
        object? input, 
        string? observationType = null,
        CancellationToken ct = default)     
    { 
        var langfuseTrace = (LangfuseTraceContext)trace;
        
        // Create child activity for node execution
        var activity = _activitySource.StartActivity( 
            $"node.{nodeName}", 
            ActivityKind.Internal, 
            langfuseTrace.Activity?.Context ?? default
        );
        
        if (activity != null)
        { 
            activity.SetTag("node.name", nodeName); 
            activity.SetTag("langfuse.observation.type", observationType ?? "span"); 
            activity.SetTag("langfuse.span.name", nodeName); 
            // Serialize input if possible
            if (input != null)
            {
                try
                {
                    var inputJson = System.Text.Json.JsonSerializer.Serialize(input);
                    activity.SetTag("langfuse.observation.input", inputJson);
                }
                catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
                {
                    // Fallback to ToString if serialization fails
                    activity.SetTag("langfuse.observation.input", input.ToString());
                }
            } 
        } 
        
        return Task.FromResult<ISpanContext>( new LangfuseSpanContext(activity)); 
    }

    /// <inheritdoc />
    public Task EndSpanAsync( 
        ISpanContext span, 
        object? output, 
        Exception? error = null, 
        CancellationToken ct = default) 
    { 
        var langfuseSpan = (LangfuseSpanContext)span; 
        if (langfuseSpan.Activity != null) 
        { 
            // Add output
            if (output != null)
            {
                try
                {
                    var outputJson = System.Text.Json.JsonSerializer.Serialize(output);
                    langfuseSpan.Activity.SetTag("langfuse.observation.output", outputJson);
                }
                catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
                {
                    // Fallback to ToString if serialization fails
                    langfuseSpan.Activity.SetTag("langfuse.observation.output", output.ToString());
                }
            } 
            
            // Handle errors
            if (error != null)
            {
                langfuseSpan.Activity.SetStatus(ActivityStatusCode.Error, error.Message); 
                //langfuseSpan.Activity.RecordException(error); 
                langfuseSpan.Activity.SetTag("error.type", error.GetType().Name); 
                langfuseSpan.Activity.SetTag("error.message", error.Message); 
                langfuseSpan.Activity.SetTag("error.stacktrace", error.StackTrace);
            }
            else
            {
                langfuseSpan.Activity.SetStatus(ActivityStatusCode.Ok);
            } 
            
            langfuseSpan.Activity.Stop(); 
        } 
        
        return Task.CompletedTask; 
    }

    /// <inheritdoc />
    /// <remarks>
    /// Creates an OpenTelemetry Activity with Langfuse-specific tags following the semantic conventions
    /// for LLM observability. Captures request parameters, messages, and tool definitions.
    /// </remarks>
    public Task<ILLMSpanContext> StartLLMSpanAsync( 
        ITraceContext trace, 
        string modelName, 
        ChatRequest request, 
        CancellationToken ct = default) 
    { 
        var langfuseTrace = (LangfuseTraceContext)trace; 
        // Create LLM generation span - critical for Langfuse
        var activity = _activitySource.StartActivity( 
            "chat.completion", 
            ActivityKind.Client, 
            langfuseTrace.Activity?.Context ?? default);
        
        if (activity != null) 
        { 
            // Langfuse semantic conventions (based on the GitHub issue)
            activity.SetTag("gen_ai.system", "openai"); // or detect from model
            activity.SetTag("gen_ai.operation.name", "chat"); 
            activity.SetTag("gen_ai.endpoint", "openai.chat.completions"); 
            activity.SetTag("gen_ai.request.model", modelName); 
            activity.SetTag("gen_ai.application_name", _applicationName); 
            
            // Request parameters
            activity.SetTag("gen_ai.request.temperature", request.Temperature ?? 1.0f); 
            activity.SetTag("gen_ai.request.max_tokens", request.MaxTokens ?? -1); 
            activity.SetTag("gen_ai.request.top_p", request.TopP ?? 1.0f); 
            activity.SetTag("gen_ai.request.presence_penalty", request.PresencePenalty ?? 0.0f); 
            activity.SetTag("gen_ai.request.frequency_penalty", request.FrequencyPenalty ?? 0.0f); 
            activity.SetTag("gen_ai.request.is_stream", false); 
            
            if (request.User != null) 
                activity.SetTag("gen_ai.request.user", request.User); 
            
            if (request.Seed.HasValue) 
                activity.SetTag("gen_ai.request.seed", request.Seed.Value); 
            
            // Add messages as structured data
            for (int i = 0; i < request.Messages.Count; i++)
            {
                var msg = request.Messages[i]; 
                var content = string.Join("", msg.Parts.OfType<TextContent>().Select(p => p.Text)); 
                activity.SetTag($"gen_ai.prompt.{i}.role", msg.Role.ToString().ToLower()); 
                activity.SetTag($"gen_ai.prompt.{i}.content", content);
            } 
            
            // Tools if present
            if (request.Tools?.Tools?.Count > 0)
            {
                activity.SetTag("gen_ai.request.tools_count", request.Tools.Tools.Count);
                for (int i = 0; i < request.Tools.Tools.Count; i++)
                {
                    var tool = request.Tools.Tools[i]; 
                    activity.SetTag($"gen_ai.request.tool.{i}.name", tool.Name); 
                    activity.SetTag($"gen_ai.request.tool.{i}.description", tool.Description);
                }
            } 
            
            // Important for Langfuse
            activity.SetTag("scope", "langfuse.otel.tracing"); 
            activity.SetTag("langfuse.observation.type", "generation"); 
        } 
        
        return Task.FromResult<ILLMSpanContext>( new LangfuseLLMSpanContext(activity)); 
    }

    /// <inheritdoc />
    /// <remarks>
    /// Captures completion response including generated content, tool calls, and token usage.
    /// Token usage data is critical for Langfuse's cost tracking and analytics features.
    /// </remarks>
    public Task EndLLMSpanAsync( 
        ILLMSpanContext span, 
        ChatCompletion? completion, 
        Exception? error = null, 
        CancellationToken ct = default) 
    { 
        var langfuseSpan = (LangfuseLLMSpanContext)span;
        var activity = langfuseSpan.Activity;
        
        if (activity != null) 
        { 
            if (completion != null) 
            { 
                // Response ID
                // Note: You'd need to extract this from raw response if available
                // activity.SetTag("gen_ai.response.id", completion.ResponseId);
                
                // Finish reason
                activity.SetTag("gen_ai.response.finish_reason", completion.FinishReason.ToString().ToLower()); 
                
                // Completion content
                var completionContent = string.Join("", completion.Message.Parts.OfType<TextContent>().Select(p => p.Text)); 
                activity.SetTag("gen_ai.completion.0.role", "assistant"); 
                activity.SetTag("gen_ai.completion.0.content", completionContent); 
                
                // Tool calls if present
                if (completion.ToolCalls?.Count > 0)
                {
                    for (int i = 0; i < completion.ToolCalls.Count; i++)
                    {
                        var toolCall = completion.ToolCalls[i]; 
                        activity.SetTag($"gen_ai.completion.tool_call.{i}.id", toolCall.Id); 
                        activity.SetTag($"gen_ai.completion.tool_call.{i}.name", toolCall.Name); 
                        activity.SetTag($"gen_ai.completion.tool_call.{i}.arguments", toolCall.ArgumentsJson);
                    }
                } 
                
                // Usage/tokens - CRITICAL for Langfuse cost tracking
                if (completion.Usage != null)
                {
                    activity.SetTag("gen_ai.usage.input_tokens", completion.Usage.PromptTokens); 
                    activity.SetTag("gen_ai.usage.output_tokens", completion.Usage.CompletionTokens); 
                    activity.SetTag("gen_ai.usage.total_tokens", completion.Usage.TotalTokens);
                } 
                
                // Model from response
                if (completion.Model != null)
                {
                    activity.SetTag("gen_ai.response.model", completion.Model);
                } 
                
                activity.SetStatus(ActivityStatusCode.Ok); 
            }

            if (error != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, error.Message); 
                //activity.RecordException(error); 
                activity.SetTag("error.type", error.GetType().Name); 
                activity.SetTag("error.message", error.Message);
            } 
            
            activity.Stop(); 
        } 
        
        return Task.CompletedTask; 
    }

    /// <summary>
    /// Disposes the ActivitySource to release resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _activitySource?.Dispose();
        _disposed = true;
    }
}
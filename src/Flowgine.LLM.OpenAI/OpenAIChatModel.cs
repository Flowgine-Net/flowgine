using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using Flowgine.LLM.Abstractions;

using OpenAI.Chat;
using ChatCompletion = OpenAI.Chat.ChatCompletion;
using ChatFinishReason = OpenAI.Chat.ChatFinishReason;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace Flowgine.LLM.OpenAI;

/// <summary>
/// OpenAI implementation of <see cref="IChatModel"/> using the OpenAI SDK 2.x.
/// </summary>
public sealed class OpenAIChatModel : IChatModel
{
    private readonly ChatClient _client;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIChatModel"/> class.
    /// </summary>
    /// <param name="client">The OpenAI chat client to use for API calls.</param>
    public OpenAIChatModel(ChatClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<Flowgine.LLM.Abstractions.ChatCompletion> GenerateAsync(
        ChatRequest request, CancellationToken ct = default)
    {
        ValidateRequest(request);
        
        var input = ToOpenAIInput(request.Messages);
        var settings = BuildCompletionOptions(request);
        
        ChatCompletion result= await _client.CompleteChatAsync(input, settings, ct);

        return FromOpenAI(result);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ValidateRequest(request);
        
        var input = ToOpenAIInput(request.Messages);
        var settings = BuildCompletionOptions(request);

        // Variables for aggregating the final response
        var contentBuilder = new StringBuilder();
        var toolCallsBuilder = new Dictionary<int, ToolCallAccumulator>();
        string? modelId = null;
        
        await foreach (var update in _client.CompleteChatStreamingAsync(input, settings, ct))
        {
            // 1. Process content updates (text tokens)
            if (update.ContentUpdate != null)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    var text = contentPart.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        contentBuilder.Append(text);
                        yield return new TokenChunk(text);
                    }
                }
            }
            
            // 2. Capture model ID
            if (!string.IsNullOrEmpty(update.Model))
            {
                modelId = update.Model;
            }
            
            // 3. Process tool call updates
            if (update.ToolCallUpdates != null)
            {
                foreach (var toolUpdate in update.ToolCallUpdates)
                {
                    var index = toolUpdate.Index;
                    
                    if (!toolCallsBuilder.ContainsKey(index))
                    {
                        toolCallsBuilder[index] = new ToolCallAccumulator
                        {
                            Id = toolUpdate.ToolCallId ?? "",
                            Name = toolUpdate.FunctionName ?? "",
                            ArgumentsBuilder = new StringBuilder()
                        };
                    }
                    
                    var accumulator = toolCallsBuilder[index];
                    
                    // Update ID if available
                    if (!string.IsNullOrEmpty(toolUpdate.ToolCallId))
                        accumulator.Id = toolUpdate.ToolCallId;
                    
                    // Update function name
                    if (!string.IsNullOrEmpty(toolUpdate.FunctionName))
                        accumulator.Name = toolUpdate.FunctionName;
                    
                    // Accumulate arguments
                    if (toolUpdate.FunctionArgumentsUpdate != null)
                    {
                        var argsUpdate = toolUpdate.FunctionArgumentsUpdate.ToString();
                        accumulator.ArgumentsBuilder.Append(argsUpdate);
                        
                        // Emit delta event
                        yield return new ToolCallDelta(
                            accumulator.Name,
                            argsUpdate,
                            accumulator.Id
                        );
                    }
                }
            }
            
            // 4. Process finish reason (end of stream)
            if (update.FinishReason.HasValue)
            {
                // Build final completion
                var message = new Flowgine.LLM.Abstractions.ChatMessage(
                    ChatRole.Assistant,
                    [new TextContent(contentBuilder.ToString())]
                );
                
                var toolCalls = toolCallsBuilder.Values
                    .Select(acc => new ToolCall(
                        acc.Name,
                        acc.ArgumentsBuilder.ToString(),
                        acc.Id
                    ))
                    .ToList();
                
                var finishReason = MapFinishReason(update.FinishReason.Value);
                
                var completion = new Flowgine.LLM.Abstractions.ChatCompletion(
                    message,
                    toolCalls,
                    modelId,
                    finishReason,
                    null, // Usage is not available during streaming
                    null  // RawJson is not available during streaming
                );
                
                yield return new Completed(completion);
            }
        }
    }

    // ---------- Type mapping ----------
    private ChatCompletionOptions BuildCompletionOptions(ChatRequest request)
    {
        var options = new ChatCompletionOptions();
        
        // Basic parameters
        ApplyIfHasValue(request.Temperature, v => options.Temperature = v);
        ApplyIfHasValue(request.MaxTokens,   v => options.MaxOutputTokenCount = v);
        ApplyIfHasValue(request.TopP,        v => options.TopP = v);
        ApplyIfHasValue(request.FrequencyPenalty, v => options.FrequencyPenalty = v);
        ApplyIfHasValue(request.PresencePenalty,  v => options.PresencePenalty = v);
        
        // Seed is experimental in OpenAI SDK 2.x
#pragma warning disable OPENAI001
        ApplyIfHasValue(request.Seed,        v => options.Seed = v);
#pragma warning restore OPENAI001
        
        // Stop sequences
        if (request.Stop != null)
        {
            foreach (var stopSeq in request.Stop)
            {
                options.StopSequences.Add(stopSeq);
            }
        }
        
        // User tracking
        if (!string.IsNullOrEmpty(request.User))
        {
            options.EndUserId = request.User;
        }
        
        // Note: TopK is not supported by OpenAI and is ignored
        
        // Map tools if provided
        if (request.Tools != null)
        {
            foreach (var tool in request.Tools.Tools)
            {
                var chatTool = ChatTool.CreateFunctionTool(
                    functionName: tool.Name,
                    functionDescription: tool.Description,
                    functionParameters: tool.Parameters != null 
                        ? BinaryData.FromString(JsonSerializer.Serialize(tool.Parameters))
                        : null,
                    functionSchemaIsStrict: tool.Strict ?? false
                );
                
                options.Tools.Add(chatTool);
            }
            
            // Map tool choice
            options.ToolChoice = request.Tools.Choice switch
            {
                ToolChoice.Auto => ChatToolChoice.CreateAutoChoice(),
                ToolChoice.Required => ChatToolChoice.CreateRequiredChoice(),
                ToolChoice.None => ChatToolChoice.CreateNoneChoice(),
                ToolChoice.Specific when !string.IsNullOrEmpty(request.Tools.ChoiceName) 
                    => ChatToolChoice.CreateFunctionChoice(request.Tools.ChoiceName),
                _ => ChatToolChoice.CreateAutoChoice()
            };
        }

        return options;
    }
    
    // Reusable helper that assigns only when the request provided a value.
    // No instance defaults are considered.
    private static void ApplyIfHasValue<T>(T? reqVal, Action<T> assign) where T : struct
    {
        if (reqVal.HasValue)
        {
            assign(reqVal.Value);
        }
        // else: omit -> server default on OpenAI side
    }

    private static IEnumerable<ChatMessage> ToOpenAIInput(IReadOnlyList<Flowgine.LLM.Abstractions.ChatMessage> msgs)
    {
        foreach (var m in msgs)
        {
            var text = string.Join("", m.Parts.OfType<TextContent>().Select(p => p.Text));
            yield return m.Role switch
            {
                ChatRole.System    => ChatMessage.CreateSystemMessage(text),
                ChatRole.User      => ChatMessage.CreateUserMessage(text),
                ChatRole.Assistant => ChatMessage.CreateAssistantMessage(text),
                _ => ChatMessage.CreateUserMessage(text),
            };
        }
    }
    
    private static Flowgine.LLM.Abstractions.ChatCompletion FromOpenAI(ChatCompletion completion)
    {
        var txt = completion.Content?.FirstOrDefault()?.Text ?? "";
        var message = new Flowgine.LLM.Abstractions.ChatMessage(
            ChatRole.Assistant, [new TextContent(txt)]);

        var tools = completion.ToolCalls?.Select(tc =>
            new ToolCall(tc.FunctionName, tc.FunctionArguments.ToString(), tc.Id)).ToArray() ?? [];

        // Map finish reason
        var finishReason = MapFinishReason(completion.FinishReason);
        
        // Map usage info
        UsageInfo? usage = null;
        if (completion.Usage != null)
        {
            usage = new UsageInfo(
                PromptTokens: completion.Usage.InputTokenCount,
                CompletionTokens: completion.Usage.OutputTokenCount,
                TotalTokens: completion.Usage.TotalTokenCount
            );
        }

        return new Flowgine.LLM.Abstractions.ChatCompletion(
            message, 
            tools, 
            completion.Model, 
            finishReason,
            usage,
            completion.Content?.ToString()
        );
    }
    
    private static void ValidateRequest(ChatRequest request)
    {
        if (request.Messages.Count == 0)
            throw new ValidationException("Messages cannot be empty.", nameof(request.Messages));
            
        if (request.Temperature is < 0f or > 2f)
            throw new ValidationException(
                "Temperature must be between 0.0 and 2.0 for OpenAI.", 
                nameof(request.Temperature));
                
        if (request.TopP is < 0f or > 1f)
            throw new ValidationException(
                "TopP must be between 0.0 and 1.0.", 
                nameof(request.TopP));
                
        if (request.MaxTokens is <= 0)
            throw new ValidationException(
                "MaxTokens must be positive.", 
                nameof(request.MaxTokens));
                
        if (request.FrequencyPenalty is < 0f or > 2f)
            throw new ValidationException(
                "FrequencyPenalty must be between 0.0 and 2.0.", 
                nameof(request.FrequencyPenalty));
                
        if (request.PresencePenalty is < 0f or > 2f)
            throw new ValidationException(
                "PresencePenalty must be between 0.0 and 2.0.", 
                nameof(request.PresencePenalty));
    }
    
    private static Flowgine.LLM.Abstractions.FinishReason MapFinishReason(ChatFinishReason? reason)
    {
        if (reason == null)
            return Flowgine.LLM.Abstractions.FinishReason.Unknown;
            
        return reason.Value.ToString() switch
        {
            "Stop" => Flowgine.LLM.Abstractions.FinishReason.Stop,
            "Length" => Flowgine.LLM.Abstractions.FinishReason.Length,
            "ToolCalls" => Flowgine.LLM.Abstractions.FinishReason.ToolCalls,
            "ContentFilter" => Flowgine.LLM.Abstractions.FinishReason.ContentFilter,
            _ => Flowgine.LLM.Abstractions.FinishReason.Unknown
        };
    }
    
    /// <summary>
    /// Helper class for accumulating tool call data during streaming.
    /// </summary>
    private class ToolCallAccumulator
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public StringBuilder ArgumentsBuilder { get; set; } = new();
    }
}
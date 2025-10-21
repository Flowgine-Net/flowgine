using System.Text.Json;
using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._09_ToolCalling;

/// <summary>
/// Assistant node that uses tool calling to answer questions
/// </summary>
public sealed class AssistantNode : AsyncNode<AgentState>
{
    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var openAIProvider = runtime.Get<IOpenAIProvider>();
        var llm = openAIProvider.GetModel();
        
        // Define available tools
        var tools = new ToolConfiguration
        {
            Tools = 
            [
                new ToolDefinition
                {
                    Name = "get_current_time",
                    Description = "Gets the current time in a specific timezone. Call this when the user asks about time.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            timezone = new 
                            { 
                                type = "string",
                                description = "The timezone identifier (e.g., 'Europe/Prague', 'America/New_York', 'UTC')",
                                @default = "UTC"
                            }
                        }
                    }
                },
                new ToolDefinition
                {
                    Name = "get_weather",
                    Description = "Gets the current weather for a city. Call this when the user asks about weather.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            city = new 
                            { 
                                type = "string",
                                description = "The city name (e.g., 'Prague', 'London', 'New York')"
                            },
                            unit = new 
                            { 
                                type = "string",
                                description = "Temperature unit",
                                @enum = new[] { "celsius", "fahrenheit" },
                                @default = "celsius"
                            }
                        },
                        required = new[] { "city" }
                    }
                },
                new ToolDefinition
                {
                    Name = "calculate",
                    Description = "Performs mathematical calculations. Call this when the user asks to calculate something.",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            expression = new 
                            { 
                                type = "string",
                                description = "The mathematical expression to evaluate (e.g., '2 + 2', '10 * 5 + 3')"
                            }
                        },
                        required = new[] { "expression" }
                    }
                }
            ],
            Choice = ToolChoice.Auto
        };
        
        // Create request with tools
        var request = new ChatRequest(
            Messages: state.Messages,
            Tools: tools
        );
        
        System.Console.WriteLine("\n🤖 Assistant is thinking...");
        
        // Call LLM
        var completion = await llm.GenerateAsync(request, ct);
        
        // Add assistant response to history
        var updatedMessages = new List<ChatMessage>(state.Messages) 
        { 
            completion.Message 
        };
        
        // Check if LLM wants to call tools
        if (completion.ToolCalls.Count > 0)
        {
            System.Console.WriteLine($"\n🔧 Assistant wants to call {completion.ToolCalls.Count} tool(s):");
            
            // Execute each tool call
            foreach (var toolCall in completion.ToolCalls)
            {
                System.Console.WriteLine($"   • {toolCall.Name} (ID: {toolCall.Id})");
                
                // Execute the tool
                var toolResult = ExecuteTool(toolCall.Name, toolCall.ArgumentsJson);
                
                System.Console.WriteLine($"     → Result: {toolResult}");
                
                // Add tool result as a proper Tool message with toolCallId
                updatedMessages.Add(ChatMessage.Tool(toolResult, toolCall.Id));
            }
            
            // Call LLM again with tool results to get final answer
            System.Console.WriteLine("\n🤖 Assistant is formulating response with tool results...");
            
            var finalRequest = new ChatRequest(
                Messages: updatedMessages
            );
            
            var finalCompletion = await llm.GenerateAsync(finalRequest, ct);
            updatedMessages.Add(finalCompletion.Message);
            
            var responseText = string.Join("", 
                finalCompletion.Message.Parts.OfType<TextContent>().Select(p => p.Text));
            
            System.Console.WriteLine($"\n💬 Assistant: {responseText}");
            
            return Update.Of<AgentState>()
                .Set(x => x.Messages, updatedMessages)
                .Set(x => x.FinalResponse, responseText);
        }
        else
        {
            // No tool calls, just return the response
            var responseText = string.Join("", 
                completion.Message.Parts.OfType<TextContent>().Select(p => p.Text));
            
            System.Console.WriteLine($"\n💬 Assistant: {responseText}");
            
            return Update.Of<AgentState>()
                .Set(x => x.Messages, updatedMessages)
                .Set(x => x.FinalResponse, responseText);
        }
    }
    
    /// <summary>
    /// Execute a tool call and return the result
    /// </summary>
    private string ExecuteTool(string toolName, string argumentsJson)
    {
        try
        {
            var args = JsonDocument.Parse(argumentsJson);
            
            return toolName switch
            {
                "get_current_time" => GetCurrentTime(args),
                "get_weather" => GetWeather(args),
                "calculate" => Calculate(args),
                _ => $"Error: Unknown tool '{toolName}'"
            };
        }
        catch (Exception ex)
        {
            return $"Error executing tool: {ex.Message}";
        }
    }
    
    private string GetCurrentTime(JsonDocument args)
    {
        var timezone = args.RootElement.TryGetProperty("timezone", out var tz) 
            ? tz.GetString() ?? "UTC" 
            : "UTC";
        
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var time = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
            return $"The current time in {timezone} is {time:HH:mm:ss} on {time:dddd, MMMM d, yyyy}";
        }
        catch
        {
            var utcTime = DateTime.UtcNow;
            return $"Could not find timezone '{timezone}'. UTC time is {utcTime:HH:mm:ss} on {utcTime:dddd, MMMM d, yyyy}";
        }
    }
    
    private string GetWeather(JsonDocument args)
    {
        if (!args.RootElement.TryGetProperty("city", out var cityProp))
        {
            return "Error: City parameter is required";
        }
        
        var city = cityProp.GetString();
        var unit = args.RootElement.TryGetProperty("unit", out var unitProp) 
            ? unitProp.GetString() ?? "celsius" 
            : "celsius";
        
        // Simulate weather data (in real app, you'd call a weather API)
        var random = new Random(city?.GetHashCode() ?? 0);
        var temp = unit == "fahrenheit" 
            ? random.Next(32, 95) 
            : random.Next(0, 35);
        
        var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy", "windy" };
        var condition = conditions[random.Next(conditions.Length)];
        
        var tempUnit = unit == "fahrenheit" ? "°F" : "°C";
        
        return $"The weather in {city} is {condition} with a temperature of {temp}{tempUnit}";
    }
    
    private string Calculate(JsonDocument args)
    {
        if (!args.RootElement.TryGetProperty("expression", out var exprProp))
        {
            return "Error: Expression parameter is required";
        }
        
        var expression = exprProp.GetString();
        
        try
        {
            // Simple calculator (in real app, use a proper expression parser)
            // For demo purposes, we'll use DataTable.Compute
            var table = new System.Data.DataTable();
            var result = table.Compute(expression!, null);
            
            return $"{expression} = {result}";
        }
        catch (Exception ex)
        {
            return $"Error calculating '{expression}': {ex.Message}";
        }
    }
}


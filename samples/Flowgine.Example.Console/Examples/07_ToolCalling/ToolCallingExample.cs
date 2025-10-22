using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._07_ToolCalling;

/// <summary>
/// Demonstrates the new type-safe Tools API
/// </summary>
public static class ToolCallingExample
{
    /// <summary>
    /// Example 1: Simple tool with no parameters
    /// </summary>
    public static ToolConfiguration SimpleToolExample()
    {
        return new ToolConfiguration
        {
            Tools = 
            [
                new ToolDefinition
                {
                    Name = "get_current_time",
                    Description = "Returns the current date and time"
                }
            ],
            Choice = ToolChoice.Auto
        };
    }
    
    /// <summary>
    /// Example 2: Tool with JSON Schema parameters
    /// </summary>
    public static ToolConfiguration WeatherToolExample()
    {
        return new ToolConfiguration
        {
            Tools = 
            [
                new ToolDefinition
                {
                    Name = "get_weather",
                    Description = "Get current weather for a specific location",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new 
                            { 
                                type = "string", 
                                description = "The city name, e.g. 'London', 'New York'" 
                            },
                            unit = new 
                            { 
                                type = "string", 
                                description = "Temperature unit",
                                @enum = new[] { "celsius", "fahrenheit" },
                                @default = "celsius"
                            }
                        },
                        required = new[] { "location" }
                    }
                }
            ]
        };
    }
    
    /// <summary>
    /// Example 3: Multiple tools with strict mode (OpenAI structured outputs)
    /// </summary>
    public static ToolConfiguration MultipleToolsExample()
    {
        var searchTool = new ToolDefinition
        {
            Name = "search_database",
            Description = "Search for products in the database",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query = new 
                    { 
                        type = "string",
                        description = "Search query string"
                    },
                    category = new 
                    { 
                        type = "string",
                        description = "Product category to filter by",
                        @enum = new[] { "electronics", "clothing", "food", "books" }
                    },
                    max_results = new 
                    { 
                        type = "integer",
                        description = "Maximum number of results to return",
                        minimum = 1,
                        maximum = 100,
                        @default = 10
                    }
                },
                required = new[] { "query" },
                additionalProperties = false
            },
            Strict = true  // OpenAI strict mode for structured outputs
        };
        
        var orderTool = new ToolDefinition
        {
            Name = "create_order",
            Description = "Create a new order for products",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    product_ids = new 
                    { 
                        type = "array",
                        items = new { type = "string" },
                        description = "List of product IDs to order"
                    },
                    quantity = new 
                    { 
                        type = "integer",
                        minimum = 1,
                        description = "Quantity to order"
                    },
                    shipping_address = new
                    {
                        type = "object",
                        properties = new
                        {
                            street = new { type = "string" },
                            city = new { type = "string" },
                            country = new { type = "string" }
                        },
                        required = new[] { "street", "city", "country" }
                    }
                },
                required = new[] { "product_ids", "quantity", "shipping_address" },
                additionalProperties = false
            },
            Strict = true
        };
        
        return new ToolConfiguration
        {
            Tools = [searchTool, orderTool],
            Choice = ToolChoice.Auto
        };
    }
    
    /// <summary>
    /// Example 4: Force specific tool call
    /// </summary>
    public static ToolConfiguration ForceToolCallExample()
    {
        return new ToolConfiguration
        {
            Tools = 
            [
                new ToolDefinition
                {
                    Name = "calculate_price",
                    Description = "Calculate total price with tax",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            base_price = new { type = "number" },
                            tax_rate = new { type = "number", minimum = 0, maximum = 1 }
                        },
                        required = new[] { "base_price", "tax_rate" }
                    }
                },
                new ToolDefinition
                {
                    Name = "apply_discount",
                    Description = "Apply discount code",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            discount_code = new { type = "string" }
                        },
                        required = new[] { "discount_code" }
                    }
                }
            ],
            Choice = ToolChoice.Specific,
            ChoiceName = "calculate_price"  // Force LLM to use this specific tool
        };
    }
    
    /// <summary>
    /// Example 5: Complete usage in ChatRequest
    /// </summary>
    public static ChatRequest CreateToolCallRequest()
    {
        var tools = new ToolConfiguration
        {
            Tools = 
            [
                new ToolDefinition
                {
                    Name = "get_weather",
                    Description = "Get current weather",
                    Parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new { type = "string" }
                        },
                        required = new[] { "location" }
                    }
                }
            ],
            Choice = ToolChoice.Auto
        };
        
        return new ChatRequest(
            Messages: 
            [
                ChatMessage.System("You are a helpful assistant with access to weather information."),
                ChatMessage.User("What's the weather like in Prague?")
            ],
            Temperature: 0.7f,
            MaxTokens: 500,
            Tools: tools
        );
    }
}


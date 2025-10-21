using Flowgine.Abstractions;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Example.Console.Examples._10_PromptTemplates;

public sealed class ContentGeneratorNode : AsyncNode<AgentState>
{
    // Define reusable prompt template as a static field
    private static readonly PromptTemplate _promptTemplate = PromptTemplate.FromTemplate(
        "Write a short {style} paragraph about {topic}. Keep it under 100 words."
    );

    public override async ValueTask<object?> InvokeAsync(
        AgentState state, Runtime runtime, CancellationToken ct = default)
    {
        var chatModel = runtime.Get<IChatModel>();
        
        // Format the prompt using the template
        var prompt = _promptTemplate.Format(new 
        { 
            topic = state.Topic,
            style = state.Style
        });
        
        System.Console.WriteLine($"\n📝 Generated prompt: {prompt}\n");
        
        // Create request with formatted prompt
        var request = new ChatRequest(
            Messages: [ChatMessage.User(prompt)],
            Temperature: 0.7f,
            MaxTokens: 200
        );
        
        var response = await chatModel.GenerateAsync(request, ct);
        
        var generatedText = response.Message.Parts
            .OfType<TextContent>()
            .FirstOrDefault()?.Text ?? "";
        
        System.Console.WriteLine($"✨ Generated content:\n{generatedText}\n");
        
        return Update.Of<AgentState>()
            .Set(s => s.GeneratedContent, generatedText);
    }
}


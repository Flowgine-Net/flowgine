namespace Flowgine.LLM.OpenAI;

public sealed class OpenAIChatOptions
{
    public string ApiKey { get; set; } = "";
    public string? BaseUrl { get; set; }          // custom endpoint (třeba proxy)
    public string DefaultModel { get; set; } = "gpt-4o-mini";
    public float? DefaultTemperature { get; set; }
    public int? DefaultMaxTokens { get; set; }
}
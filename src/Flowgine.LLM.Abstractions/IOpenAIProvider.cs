namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Factory for producing <see cref="IChatModel"/> instances backed by OpenAI.
/// Implementations typically hold shared HTTP/OpenAI client state (connection pooling).
/// </summary>
public interface IOpenAIProvider
{
    /// <summary>
    /// Gets a chat model using the configured defaults (e.g., DefaultModel from options).
    /// </summary>
    /// <returns>An <see cref="IChatModel"/> bound to the default model.</returns>
    IChatModel GetModel();

    /// <summary>
    /// Gets a chat model bound to a specific model identifier/version.
    /// </summary>
    /// <param name="model">The OpenAI model id (e.g., "gpt-5.1-mini"). Must be non-empty.</param>
    /// <returns>An <see cref="IChatModel"/> bound to the given model.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="model"/> is null or whitespace.</exception>
    IChatModel GetModel(string model);
}
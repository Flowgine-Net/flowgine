using Microsoft.Extensions.Options;
using OpenAI;
using Flowgine.LLM.Abstractions;

namespace Flowgine.LLM.OpenAI;

/// <summary>
/// Factory (provider) for creating <see cref="IChatModel"/> instances backed by OpenAI.
/// Holds a single root <see cref="OpenAIClient"/> to reuse HTTP pipeline, auth, and connection pooling.
/// </summary>
/// <remarks>
/// Typical DI lifetime is <b>Singleton</b>. Use <see cref="GetModel()"/> for the configured default model,
/// or <see cref="GetModel(string)"/> to target a specific model identifier/version.
/// </remarks>
public sealed class OpenAIProvider : IOpenAIProvider
{
    /// <summary>
    /// Captured application-level defaults and credentials (e.g., API key, default model).
    /// </summary>
    private readonly OpenAIChatOptions _options;

    /// <summary>
    /// Root OpenAI client reused across all chat models to minimize allocations and socket churn.
    /// Individual <c>ChatClient</c> instances are created per model id from this root.
    /// </summary>
    private readonly OpenAIClient _rootClient;
    
    /// <summary>
    /// Creates a new <see cref="OpenAIProvider"/>.
    /// </summary>
    /// <param name="options">Strongly-typed options for OpenAI configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the API key is missing or empty.</exception>
    public OpenAIProvider(IOptions<OpenAIChatOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            // Provider cannot operate without credentials. Fail fast at startup.
            throw new InvalidOperationException("OpenAI API key is required.");
        }
        
        // Create API key credential
        var credential = new System.ClientModel.ApiKeyCredential(_options.ApiKey);
        
        // Validate BaseUrl if provided and create client with custom endpoint
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException($"Invalid BaseUrl provided: '{_options.BaseUrl}'. Must be a valid absolute URI.");
            }
            
            // Create client with custom endpoint (e.g., Azure OpenAI, proxy, or local LLM server)
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = baseUri
            };
            _rootClient = new OpenAIClient(credential, clientOptions);
        }
        else
        {
            // Use default OpenAI endpoint
            _rootClient = new OpenAIClient(credential);
        }
    }
    
    /// <summary>
    /// Returns an <see cref="IChatModel"/> bound to the configured default model.
    /// </summary>
    /// <returns>Chat model using <see cref="OpenAIChatOptions.DefaultModel"/>.</returns>
    /// <remarks>
    /// If <see cref="OpenAIChatOptions.DefaultModel"/> is not set, the underlying SDK may throw when used.
    /// Consider validating the default model during application startup.
    /// </remarks>
    public IChatModel GetModel()
    {
        // Derive a lightweight ChatClient for the default model from the root client.
        var model = _rootClient.GetChatClient(_options.DefaultModel);
        return new OpenAIChatModel(model, _options.DefaultModel);
    }

    /// <summary>
    /// Returns an <see cref="IChatModel"/> bound to a specific model identifier/version.
    /// </summary>
    /// <param name="model">The OpenAI model id (e.g., "gpt-5.1-mini" or a dated variant).</param>
    /// <returns>Chat model pinned to the supplied <paramref name="model"/>.</returns>
    /// <remarks>
    /// Prefer this overload when you need to target a specific, immutable model version for reproducibility.
    /// </remarks>
    /// <exception cref="ArgumentException">If <paramref name="model"/> is null or whitespace.</exception>
    public IChatModel GetModel(string model)
    {
        // Validate early to avoid opaque downstream errors.
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model id must be provided.", nameof(model));

        var chatModel = _rootClient.GetChatClient(model);
        return new OpenAIChatModel(chatModel, model);
    }
}


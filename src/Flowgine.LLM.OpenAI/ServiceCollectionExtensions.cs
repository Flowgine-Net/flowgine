using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Flowgine.LLM.Abstractions;

namespace Flowgine.LLM.OpenAI;

/// <summary>
/// Extension methods for registering OpenAI services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenAI chat services with fluent configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Fluent configuration action</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null</exception>
    /// <example>
    /// <code>
    /// services.AddOpenAI(cfg => cfg
    ///     .UseApiKey("sk-...")
    ///     .UseModel("gpt-4o-mini")
    ///     .UseTemperature(0.7f));
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenAI(
        this IServiceCollection services,
        Action<OpenAIServiceConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var configuration = new OpenAIServiceConfiguration();
        configure(configuration);

        services.Configure<OpenAIChatOptions>(options =>
        {
            options.ApiKey = configuration.Options.ApiKey;
            options.BaseUrl = configuration.Options.BaseUrl;
            options.DefaultModel = configuration.Options.DefaultModel;
            options.DefaultTemperature = configuration.Options.DefaultTemperature;
            options.DefaultMaxTokens = configuration.Options.DefaultMaxTokens;
            options.RequestTimeout = configuration.Options.RequestTimeout;
            options.MaxRetries = configuration.Options.MaxRetries;
            options.RetryDelay = configuration.Options.RetryDelay;
        });

        services.AddSingleton<IValidateOptions<OpenAIChatOptions>, OpenAIChatOptionsValidator>();
        services.AddSingleton<IOpenAIProvider, OpenAIProvider>();

        return services;
    }
    
    /// <summary>
    /// Registers OpenAI chat services with direct options configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for OpenAI options</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null</exception>
    /// <example>
    /// <code>
    /// services.AddOpenAI(options =>
    /// {
    ///     options.ApiKey = "sk-...";
    ///     options.DefaultModel = "gpt-4o-mini";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenAI(
        this IServiceCollection services,
        Action<OpenAIChatOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddSingleton<IValidateOptions<OpenAIChatOptions>, OpenAIChatOptionsValidator>();
        services.AddSingleton<IOpenAIProvider, OpenAIProvider>();

        return services;
    }
    
    /// <summary>
    /// Registers OpenAI chat services using configuration section from appsettings.json.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration section containing OpenAI settings</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    /// <example>
    /// <code>
    /// services.AddOpenAI(configuration.GetSection("OpenAI"));
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OpenAIChatOptions>(configuration);
        services.AddSingleton<IValidateOptions<OpenAIChatOptions>, OpenAIChatOptionsValidator>();
        services.AddSingleton<IOpenAIProvider, OpenAIProvider>();

        return services;
    }
}


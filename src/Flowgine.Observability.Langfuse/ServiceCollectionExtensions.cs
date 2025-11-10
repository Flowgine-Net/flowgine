using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;

namespace Flowgine.Observability.Langfuse;

/// <summary>
/// Extension methods for registering Langfuse observability services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Langfuse observability integration to the service collection.
    /// Configures OpenTelemetry with OTLP exporter to send traces to Langfuse.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="publicKey">The Langfuse public API key.</param>
    /// <param name="secretKey">The Langfuse secret API key.</param>
    /// <param name="applicationName">The application name for identifying traces. Default is "flowgine".</param>
    /// <param name="langfuseHost">Optional custom Langfuse host. Default is "https://cloud.langfuse.com".</param>
    /// <param name="configure">Optional action to configure additional Langfuse options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when publicKey or secretKey is null or empty.</exception>
    /// <example>
    /// <code>
    /// services.AddLangfuseObservability(
    ///     publicKey: "pk-...",
    ///     secretKey: "sk-...",
    ///     applicationName: "my-agent",
    ///     configure: options => options.EnableConsoleExporter = true
    /// );
    /// </code>
    /// </example>
    public static IServiceCollection AddLangfuseObservability(
        this IServiceCollection services,
        string publicKey,
        string secretKey,
        string applicationName = "flowgine",
        string? langfuseHost = null,
        Action<LangfuseOptions>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(publicKey))
            throw new ArgumentException("Public key cannot be null or empty", nameof(publicKey));
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty", nameof(secretKey));

        var options = new LangfuseOptions
        {
            PublicKey = publicKey, 
            SecretKey = secretKey, 
            ApplicationName = applicationName, 
            LangfuseHost = langfuseHost ?? "https://cloud.langfuse.com"
        };
        
        configure?.Invoke(options);
        services.AddSingleton(options);

        var serviceVersion = typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        
        // Configure OpenTelemetry with Langfuse exporter
        services.AddOpenTelemetry() 
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder 
                    .AddService(serviceName: applicationName, serviceVersion: serviceVersion) 
                    .AddAttributes(new[] { new KeyValuePair<string, object>("deployment.environment", options.Environment ?? "production") });
            })
            .WithTracing(tracerProviderBuilder => 
            { 
                tracerProviderBuilder 
                    .AddSource(options.ApplicationName)
                    .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporterOptions => 
                    { 
                        // Create Basic Auth header
                        var credentials = $"{options.PublicKey}:{options.SecretKey}"; 
                        var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials)); 
                        
                        exporterOptions.Endpoint = new Uri($"{options.LangfuseHost}/api/public/otel/v1/traces");
                        exporterOptions.Headers = $"Authorization=Basic {base64}"; 
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf; 
                        exporterOptions.TimeoutMilliseconds = options.TimeoutMs;
                    }); 
                
                // Optional: Add console exporter for debugging
                if (options.EnableConsoleExporter)
                {
                    tracerProviderBuilder.AddConsoleExporter();
                } 
            }); 
        
        // Register the provider
        services.AddSingleton<IObservabilityProvider>( 
            sp => new LangfuseProvider(options)); 
        
        return services;
    }
}
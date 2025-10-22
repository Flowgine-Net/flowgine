using Microsoft.Extensions.DependencyInjection;

namespace Flowgine.Abstractions;

/// <summary>
/// Provides runtime context for node execution, including a unique run identifier and service provider access.
/// </summary>
public sealed class Runtime
{
    /// <summary>
    /// Gets the unique identifier for this workflow execution run.
    /// </summary>
    public Guid RunId { get; }
    
    /// <summary>
    /// Gets the service provider for dependency injection during workflow execution.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Runtime"/> class.
    /// </summary>
    /// <param name="runId">The unique identifier for this workflow run.</param>
    /// <param name="services">The service provider for dependency injection.</param>
    public Runtime(Guid runId, IServiceProvider services)
    {
        RunId = runId;
        Services = services;
    }

    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// Throws an exception if the service is not registered.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not found.</exception>
    public T Get<T>() where T : notnull => Services.GetRequiredService<T>();
}
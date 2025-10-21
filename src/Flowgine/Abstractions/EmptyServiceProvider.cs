namespace Flowgine.Abstractions;

/// <summary>
/// A minimal implementation of <see cref="IServiceProvider"/> that always returns null.
/// Used as a default/fallback when no dependency injection is configured.
/// </summary>
public class EmptyServiceProvider : IServiceProvider
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="EmptyServiceProvider"/>.
    /// </summary>
    public static readonly EmptyServiceProvider Instance = new();
    
    private EmptyServiceProvider() { }
    
    /// <summary>
    /// Always returns null regardless of the service type requested.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>Always returns null.</returns>
    public object? GetService(Type serviceType) => null;
}
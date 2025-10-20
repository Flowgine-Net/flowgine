namespace Flowgine.Abstractions;

public class EmptyServiceProvider : IServiceProvider
{
    public static readonly EmptyServiceProvider Instance = new();
    private EmptyServiceProvider() { }
    public object? GetService(Type serviceType) => null;
}
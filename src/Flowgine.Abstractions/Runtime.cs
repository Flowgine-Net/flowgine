using Microsoft.Extensions.DependencyInjection;

namespace Flowgine.Abstractions;

public sealed class Runtime
{
    public Guid RunId { get; }
    public IServiceProvider Services { get; }

    public Runtime(Guid runId, IServiceProvider services)
    {
        RunId = runId;
        Services = services;
    }

    // syntactic sugar
    public T Get<T>() where T : notnull => Services.GetRequiredService<T>();
}
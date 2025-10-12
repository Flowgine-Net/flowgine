namespace Flowgine.Example.Console.Shared;

public interface IExample
{
    string Id { get; }        // "01-basics"
    string Title { get; }     // "Basics: partial updates"
    Task RunAsync(CancellationToken ct = default);
}

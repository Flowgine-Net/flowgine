using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._07_ContentAgent;

public class InitializeNode : AsyncNode<ContentState>
{
    public override async ValueTask<object?> InvokeAsync(
        ContentState state, Runtime runtime, CancellationToken ct = default)
    {
        return Update.Of<ContentState>().Set(s => s.BrandVoice, "");
    }
}
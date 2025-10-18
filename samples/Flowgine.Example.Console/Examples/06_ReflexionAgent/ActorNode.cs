using Flowgine.Abstractions;

namespace Flowgine.Example.Console.Examples._06_ReflexionAgent;

public class ActorNode : AsyncNode<ReflexionState>
{
    public override async ValueTask<object?> InvokeAsync(
        ReflexionState state, Runtime runtime, CancellationToken ct = default)
    {
        return Update.Of<ReflexionState>()
            .Set(s => s.Messages, state.Messages);
    }
}
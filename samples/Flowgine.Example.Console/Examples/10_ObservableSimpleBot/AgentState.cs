using Flowgine.Observability;

namespace Flowgine.Example.Console.Examples._10_ObservableSimpleBot;

public class AgentState : IObservableState
{
    public string Prompt { get; set; } = "Say 'hello from Flowgine'.";
    public string LastAnswer { get; set; } = "";

    public string? GetInput() => Prompt;
    
    public string? GetOutput() => LastAnswer;
}
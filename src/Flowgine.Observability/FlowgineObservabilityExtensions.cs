using Flowgine.Core;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Observability;

public static class FlowgineObservabilityExtensions
{
    public static ObservableCompiledFlowgine<TState> WithObservability<TState>(
        this CompiledFlowgine<TState> flow,
        IObservabilityProvider provider)
    {
        return new ObservableCompiledFlowgine<TState>(flow, provider);
    }

    public static IChatModel WithObservability(
        this IChatModel model,
        IObservabilityProvider provider,
        string modelName)
    {
        return new ObservableChatModel(model, provider, modelName);
    }
}

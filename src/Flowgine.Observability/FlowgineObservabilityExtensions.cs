using Flowgine.Core;
using Flowgine.LLM.Abstractions;

namespace Flowgine.Observability;

/// <summary>
/// Extension methods for adding observability capabilities to Flowgine flows and chat models.
/// </summary>
public static class FlowgineObservabilityExtensions
{
    /// <summary>
    /// Wraps a compiled flow with observability tracking.
    /// </summary>
    /// <typeparam name="TState">The type of state that flows through the execution.</typeparam>
    /// <param name="flow">The compiled flow to wrap.</param>
    /// <param name="provider">The observability provider for tracking execution.</param>
    /// <returns>An observable compiled flow that tracks execution events.</returns>
    public static ObservableCompiledFlowgine<TState> WithObservability<TState>(
        this CompiledFlowgine<TState> flow,
        IObservabilityProvider provider)
    {
        return new ObservableCompiledFlowgine<TState>(flow, provider);
    }

    /// <summary>
    /// Wraps a chat model with observability tracking.
    /// </summary>
    /// <param name="model">The chat model to wrap.</param>
    /// <param name="provider">The observability provider for tracking LLM calls.</param>
    /// <returns>An observable chat model that tracks all API calls.</returns>
    public static IChatModel WithObservability(
        this IChatModel model,
        IObservabilityProvider provider)
    {
        return new ObservableChatModel(model, provider);
    }
}

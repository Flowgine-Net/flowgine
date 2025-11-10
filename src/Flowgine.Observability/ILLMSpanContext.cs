namespace Flowgine.Observability;

/// <summary>
/// Represents a span context specifically for LLM (Large Language Model) operations.
/// Extends <see cref="ISpanContext"/> for tracking LLM-specific metadata and metrics.
/// </summary>
public interface ILLMSpanContext : ISpanContext { }

namespace Flowgine.Observability;

public interface ISpanContext
{
    string SpanId { get; }
    DateTime StartTime { get; }
}

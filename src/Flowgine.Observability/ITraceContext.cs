namespace Flowgine.Observability;

public interface ITraceContext
{
    string TraceId { get; }
    Dictionary<string, object> Metadata { get; }
}

namespace SilverKinetics.w80.Domain.Contracts;

public interface ISystemEventSink
{
    void Add(ISystemEvent evnt);
    void Clear();
    IEnumerable<ISystemEvent> All();
}
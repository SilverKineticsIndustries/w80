using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Services;

public class SystemEventSink
    : ISystemEventSink
{
    public void Add(ISystemEvent evnt)
    {
        _events.Add(evnt);
    }

    public IEnumerable<ISystemEvent> All()
    {
        return _events;
    }

    public void Clear()
    {
        _events.Clear();
    }

    private readonly IList<ISystemEvent> _events = [];
}

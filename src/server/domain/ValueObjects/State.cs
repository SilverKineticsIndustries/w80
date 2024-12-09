using MongoDB.Bson;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record State
{
    public ObjectId Id { get; private set; }
    public bool IsCurrent { get; set; }
    public string Name { get; private set; }
    public string Resource { get; private set; }
    public int SeqNo { get; private set; }

    public State(ObjectId id, string name, string resource, int seqNo)
    {
        Id = id;
        Name = name;
        Resource = resource;
        SeqNo = seqNo;
    }

    public State(ApplicationState applicationState)
    {
        Id = applicationState.Id;
        Name = applicationState.Name;
        Resource = applicationState.Resource;
        SeqNo = applicationState.SeqNo;
    }
}
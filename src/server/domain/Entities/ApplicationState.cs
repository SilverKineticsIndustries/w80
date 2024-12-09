using MongoDB.Bson;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class ApplicationState
    : IAggregateRoot,
    ISoftDeletionEntity
{
    public ObjectId Id { get; set; }

    public string Name { get; private set; }
    public string Resource { get; private set; }
    public int SeqNo { get; private set; }

    public ObjectId? DeactivatedBy { get; set; }
    public DateTime? DeactivatedUTC { get; set; }

    public ApplicationState(ObjectId id, string name, string resource, int seqNo)
    {
        Id = id;
        Name = name;
        Resource = resource;
        SeqNo = seqNo;
    }

    public bool IsDeactivated()
    {
        return DeactivatedUTC is not null;
    }
}
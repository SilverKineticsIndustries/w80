using MongoDB.Bson;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class ApplicationState
    : IAggregateRoot,
    ISoftDeletionEntity
{
    public ObjectId Id { get; set; }

    public string Name { get; set; }
    public string HexColor { get; set; }
    public int SeqNo { get; set; }

    public ObjectId? DeactivatedBy { get; set; }
    public DateTime? DeactivatedUTC { get; set; }

    public bool IsDeactivated()
    {
        return DeactivatedUTC is not null;
    }
}
using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IHasCreationAudit
{
    ObjectId CreatedBy { get; set;}
    DateTime CreatedUTC { set; get; }
}
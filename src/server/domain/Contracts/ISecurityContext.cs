using MongoDB.Bson;
using SilverKinetics.w80.Common;

namespace SilverKinetics.w80.Domain.Contracts;

public interface ISecurityContext
{
    ObjectId UserId { get; }
    Role Role { get; }
    bool CanAccess(ObjectId userId);
    string Language { get; }
    string Region { get; }
}
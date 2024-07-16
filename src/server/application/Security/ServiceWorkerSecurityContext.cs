using MongoDB.Bson;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Application.Security;

public class ServiceWorkerSecurityContext
    : ISecurityContext
{
    public Role Role { get; private set; }
    public ObjectId UserId { get; private set; }
    public string Language { get; } = SupportedCultures.DefaultCulture.Split("-")[0];
    public string Region { get; } = SupportedCultures.DefaultCulture.Split("-")[1];

    public ServiceWorkerSecurityContext()
    {
        UserId = ObjectId.Empty;
        Role = Role.ServiceWorker;
    }

    public bool CanAccess(ObjectId userId)
    {
        return true;
    }
}
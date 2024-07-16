using MongoDB.Bson;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationUpsertService
{
    Task<Application> InitializeAsync(CancellationToken cancellationToken = default);
    Task<Application> UpsertAsync(Application update, CancellationToken cancellationToken = default);
    Task<IEnumerable<Application>> GetOpenApplicationForUser(ObjectId userId, CancellationToken cancellationToken);
    Task<ValidationBag> ValidateAsync(Application application, CancellationToken cancellationToken = default);
}
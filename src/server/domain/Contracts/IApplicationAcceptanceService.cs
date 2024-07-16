using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationAcceptanceService
{
    void Accept(Application application, Acceptance acceptance);
    Task<IEnumerable<Application>> ArchiveAllOpenNotAcceptedApplications(Application application);
    Task<ValidationBag> ValidateAsync(Application application, Acceptance acceptance, CancellationToken cancellationToken = default);
}
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationAcceptanceService
{
    void Accept(Application application, Acceptance acceptance);
    Task<IEnumerable<Application>> ArchiveAllOpenNotAcceptedApplications(Application application);
    ValidationBag Validate(Application application, Acceptance acceptance, CancellationToken cancellationToken = default);
}
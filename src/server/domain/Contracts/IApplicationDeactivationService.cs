using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationDeactivationService
{
    void Deactivate(Application application);
    void Reactivate(Application application);
    Task<ValidationBag> ValidateForDeactivationAsync(Application application, CancellationToken cancellationToken = default);
    Task<ValidationBag> ValidateForReactivationAsync(Application application, CancellationToken cancellationToken = default);
}
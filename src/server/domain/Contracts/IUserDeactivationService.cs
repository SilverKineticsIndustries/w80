using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IUserDeactivationService
{
    Task<User> DeactivateAsync(User user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<IValidationBag> ValidateForDeactivationAsync(User user, CancellationToken cancellationToken);
}

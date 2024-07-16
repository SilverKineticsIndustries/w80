using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IUserUpsertService
{
    User Create(string email, string? nickname = null);
    Task UpsertAsync(User user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task UpsertAsync(User[] users, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken);
    Task<IValidationBag> ValidateProfileAsync(User user, CancellationToken cancellationToken);
    Task<IValidationBag> ValidateFullyAsync(User user, CancellationToken cancellationToken);
}

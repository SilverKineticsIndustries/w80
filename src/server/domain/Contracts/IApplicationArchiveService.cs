using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationArchiveService
{
    void Archive(Application application);
    void Unarchive(Application application);
    Task<ValidationBag> ValidateForArchiveAsync(Application application, CancellationToken cancellationToken = default);
    Task<ValidationBag> ValidateForUnarchiveAsync(Application application, CancellationToken cancellationToken = default);
}
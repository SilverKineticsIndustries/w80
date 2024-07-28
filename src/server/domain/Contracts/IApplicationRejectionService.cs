using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IApplicationRejectionService
{
    void Reject(Application application, Rejection rejection);
    ValidationBag Validate(Application application, Rejection rejection, CancellationToken cancellationToken = default);
}
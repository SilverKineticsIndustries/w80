using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationDeactivationService(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    ISystemEventSink systemEventSink,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IApplicationDeactivationService
{
    public void Deactivate(Entities.Application application)
    {
        systemEventSink.Add(new ApplicationDeactivatedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
        application.Deactivate(dateTimeProvider);
    }

    public void Reactivate(Entities.Application application)
    {
        systemEventSink.Add(new ApplicationReactivatedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
        application.Reactivate();
    }

    public Task<ValidationBag> ValidateForDeactivationAsync(Entities.Application application, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();
        if (application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Application cannot be deactivated because it is already in a deactivated state."]);

        return Task.FromResult(bag);
    }

    public Task<ValidationBag> ValidateForReactivationAsync(Entities.Application application, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();
        if (!application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Application cannot be reactivated because it is not in a deactivated state."]);

        return Task.FromResult(bag);
    }
}

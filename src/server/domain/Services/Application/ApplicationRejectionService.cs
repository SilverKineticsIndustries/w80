using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationRejectionService(
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    ISystemEventSink systemEventSink,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IApplicationRejectionService
{
    public void Reject(Entities.Application application, Rejection rejection)
    {
        application.Reject(dateTimeProvider, rejection);
        systemEventSink.Add(new ApplicationRejectedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
    }

    public async Task<ValidationBag> ValidateAsync(Entities.Application application, Rejection rejection, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();

        if (application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Deactivated applications cannot be rejected."]);

        if (application.IsArchived())
            bag.AddValidation(stringLocalizer["Archived applications cannot be rejected."]);

        if (application.IsRejected())
            bag.AddValidation(stringLocalizer["Application cannot be rejected because it is already in a rejected state."]);

        if (string.IsNullOrWhiteSpace(rejection.Reason))
            bag.AddValidation(stringLocalizer["Rejection reason cannot be empty."]);
        else if (rejection.Reason.Length > Rejection.ReasonMaxSize)
            bag.AddValidation(stringLocalizer["Rejection reason max length is {0} characters.", Rejection.ReasonMaxSize]);

        if (rejection.Method == RejectionMethod.None)
            bag.AddValidation(stringLocalizer["Rejection method must be provided."]);

        if (!string.IsNullOrEmpty(rejection.ResponseText) && rejection.ResponseText.Length > Rejection.ResponseTextMaxSize)
            bag.AddValidation(stringLocalizer["Rejection response text max length is {0} characters.", Rejection.ResponseTextMaxSize]);

        return bag;
    }
}

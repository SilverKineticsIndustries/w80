using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationAcceptanceService(
    IDateTimeProvider dateTimeProvider,
    ISystemEventSink systemEventSink,
    ISecurityContext securityContext,
    IApplicationUpsertService applicationUpsertService,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IApplicationAcceptanceService
{
    public void Accept(Entities.Application application, Acceptance acceptance)
    {
        application.Accept(dateTimeProvider, acceptance);
        systemEventSink.Add(new ApplicationAcceptedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
    }

    public async Task<IEnumerable<Entities.Application>> ArchiveAllOpenNotAcceptedApplications(Entities.Application application)
    {
        var openApplications = await applicationUpsertService.GetOpenApplicationForUser(securityContext.UserId, CancellationToken.None);
        openApplications = openApplications.Where(x => x.Id != application.Id).ToList();
        foreach(var openApplication in openApplications)
            openApplication.Archive(dateTimeProvider);

        return openApplications;
    }

    public async Task<ValidationBag> ValidateAsync(Entities.Application application, Acceptance acceptance, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();

        if (application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Deactivated applications cannot be accepted."]);

        if (application.IsArchived())
            bag.AddValidation(stringLocalizer["Archived applications cannot be accepted."]);

        if (application.IsAccepted())
            bag.AddValidation(stringLocalizer["Application cannot be accepted because it is already in a accepted state."]);

        if (acceptance.Method == AcceptanceMethod.None)
            bag.AddValidation(stringLocalizer["Acceptance method must be provided."]);

        if (!string.IsNullOrEmpty(acceptance.ReponseText) && acceptance.ReponseText.Length > Acceptance.ResponseTextMaxSize)
            bag.AddValidation(stringLocalizer["Acceptance text max length is {0} characters.", Acceptance.ResponseTextMaxSize]);

        return bag;
    }
}

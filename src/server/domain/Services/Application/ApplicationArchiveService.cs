using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationArchiveService(
    ISecurityContext securityContext,
    ISystemEventSink systemEventSink,
    IDateTimeProvider dateTimeProvider,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IApplicationArchiveService
{
    public void Archive(Entities.Application application)
    {
        systemEventSink.Add(new ApplicationArchivedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
        application.Archive(dateTimeProvider);
    }

    public void Unarchive(Entities.Application application)
    {
        systemEventSink.Add(new ApplicationUnarchivedEvent(securityContext.UserId, dateTimeProvider.GetUtcNow(), application));
        application.Unarchive();
    }

    public Task<ValidationBag> ValidateForArchiveAsync(Entities.Application application, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();

        if (application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Deactivated applications cannot be archived."]);

        if (application.IsRejected())
            bag.AddValidation(stringLocalizer["Rejected applications cannot be archived."]);

        if (application.IsArchived())
            bag.AddValidation(stringLocalizer["Only non-archived applications can be archived."]);

        return Task.FromResult(bag);
    }

    public Task<ValidationBag> ValidateForUnarchiveAsync(Entities.Application application, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();

        if (application.IsDeactivated())
            bag.AddValidation(stringLocalizer["Deactivated applications cannot be unarchived."]);

        if (application.IsRejected())
            bag.AddValidation(stringLocalizer["Rejected application cannot be unarchived."]);

        if (!application.IsArchived())
            bag.AddValidation(stringLocalizer["Only archived applications can be unarchived."]);

        return Task.FromResult(bag);
    }
}
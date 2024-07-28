using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Domain.Events.Application;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationUpsertService(
    ISecurityContext securityContext,
    ISystemEventSink systemEventSink,
    IDateTimeProvider dateTimeProvider,
    IApplicationRepository applicationRepo,
    IGenericReadOnlyRepository<ApplicationState> applicationStateRepo,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
    : IApplicationUpsertService
{
    public async Task<Entities.Application> InitializeAsync(CancellationToken cancellationToken)
    {
        var statuses = await applicationStateRepo.GetManyAsync(x => 1==1, cancellationToken);
        return new Entities.Application(ObjectId.GenerateNewId(), securityContext.UserId, statuses);
    }

    public async Task<IEnumerable<Entities.Application>> GetOpenApplicationForUser(ObjectId userId, CancellationToken cancellationToken)
    {
        return
            await applicationRepo
                        .GetManyAsync(x =>
                               x.UserId == userId
                            && x.ArchivedUTC == null
                            && x.DeactivatedUTC == null
                            && (x.Acceptance == null || x.Acceptance.AcceptedUTC == default )
                            && (x.Rejection == null || x.Rejection.RejectedUTC == default)
                        ,cancellationToken);
    }

    public async Task<Entities.Application> UpsertAsync(Entities.Application update, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.GetUtcNow();
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == update.Id, cancellationToken);
        if (current == null)
            systemEventSink.Add(new ApplicationInsertedEvent(securityContext.UserId, now, update));
        else
        {
            if (current.GetCurrentState().Id != update.GetCurrentState().Id)
                systemEventSink.Add(new ApplicationStateChangedEvent(securityContext.UserId, now, update, current.GetCurrentState()));

            ResetNotificationsOnShiftedEvents(now, update, current);
            systemEventSink.Add(new ApplicationUpdatedEvent(securityContext.UserId, now, update));
        }

        return update;
    }

    public async Task<ValidationBag> ValidateAsync(Entities.Application application, CancellationToken cancellationToken = default)
    {
        var bag = new ValidationBag();
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == application.Id, cancellationToken);
        if (current != null && current.IsDeactivated())
            bag.AddValidation(stringLocalizer["Deactivated application cannot be updated."]);


        if (application.UserId == ObjectId.Empty)
            bag.AddValidation(stringLocalizer["User cannot be empty."]);

        if (application.Id == ObjectId.Empty)
            bag.AddValidation(stringLocalizer["Id cannot be empty."]);

        if (string.IsNullOrWhiteSpace(application.CompanyName))
            bag.AddValidation(stringLocalizer["Company name cannot be empty."]);
        else if (application.CompanyName.Length > Entities.Application.CompanyNameMaxLength)
            bag.AddValidation(stringLocalizer["Company name max length is {0} characters.", Entities.Application.CompanyNameMaxLength]);

        if ((application.CompensationMax.HasValue || application.CompensationMin.HasValue)
                && (!application.CompensationType.HasValue || application.CompensationType == CompensationType.None))
            bag.AddValidation(stringLocalizer["Compensation type should be entered if minimum or maximum compensation is entered."]);

        if (application.CompensationMax.HasValue && application.CompensationMax.Value <= 0)
            bag.AddValidation(stringLocalizer["Compensation max cannot be a less than or equal to zero."]);
        if (application.CompensationMin.HasValue && application.CompensationMin.Value <= 0)
            bag.AddValidation(stringLocalizer["Compensation min cannot be a less than or equal to zero."]);

        if (application.CompensationMin.HasValue && application.CompensationMax.HasValue && application.CompensationMin > application.CompensationMax)
            bag.AddValidation(stringLocalizer["Compensation min cannot be greater than compensation max."]);

        if (string.IsNullOrEmpty(application.RoleDescription))
            bag.AddValidation(stringLocalizer["Role description cannot be empty."]);
        else if (application.RoleDescription.Length > Entities.Application.RoleDescriptionMaxLength)
            bag.AddValidation(stringLocalizer["Role description max length is {0} characters.", Entities.Application.RoleDescriptionMaxLength]);

        if (string.IsNullOrEmpty(application.Role))
            bag.AddValidation(stringLocalizer["Role cannot be empty."]);
        else if (application.Role.Length > Entities.Application.RoleMaxLength)
            bag.AddValidation(stringLocalizer["Role max length is {0} characters.", Entities.Application.RoleMaxLength]);

        if (!application.States.Any())
            bag.AddValidation(stringLocalizer["An application must have a set of states."]);
        else
        {
            var currentStateCount = application.States.Where(x => x.IsCurrent).Count();
            if (currentStateCount > 1)
                bag.AddValidation(stringLocalizer["An application cannot in more than one state at a time."]);
            if (currentStateCount == 0)
                bag.AddValidation(stringLocalizer["An application must be in a state."]);

            if (application.States.Select(x => x.SeqNo).Distinct().Count() != application.States.Select(x => x.SeqNo).Count())
                bag.AddValidation(stringLocalizer["Application state sequence numbers must be unique."]);

            if (application.States.Any(x => x.Id == ObjectId.Empty))
                bag.AddValidation("Application state must have a valid Id.");
            else if (application.States.Count() != application.States.Select(x => x.Id).Distinct().Count())
                bag.AddValidation("Application state must have unique Ids.");
        }

        if (application.Appointments.Count != 0)
        {
            if (application.Appointments.Any(x => x.Id == Guid.Empty))
                bag.AddValidation("Appointments must have valid Ids.");
            else if (application.Appointments.Count() != application.Appointments.Select(x => x.Id).Distinct().Count())
                bag.AddValidation("Appointments must have unique Ids.");

            foreach(var appointment in application.Appointments)
            {
                if (string.IsNullOrEmpty(appointment.Description))
                    bag.AddValidation(stringLocalizer["Appointment description cannot be empty."]);
                else if (appointment.Description.Length > Appointment.DescriptionMaxLength)
                    bag.AddValidation(stringLocalizer["Appointment description max length is {0} characters.", Appointment.DescriptionMaxLength]);
                else if (appointment.StartDateTimeUTC == default || appointment.StartDateTimeUTC.IsMinValue() || appointment.StartDateTimeUTC.IsMaxValue())
                    bag.AddValidation(stringLocalizer["Appointment start date/time cannot be empty."]);
                else if (appointment.StartDateTimeUTC.Kind != DateTimeKind.Utc)
                    bag.AddValidation(stringLocalizer["Appointment start date/time must be specified in UTC."]);
                else if (appointment.EndDateTimeUTC == default || appointment.EndDateTimeUTC.IsMinValue() || appointment.EndDateTimeUTC.IsMaxValue())
                    bag.AddValidation(stringLocalizer["Appointment end date/time cannot be empty."]);
                else if (appointment.EndDateTimeUTC.Kind != DateTimeKind.Utc)
                    bag.AddValidation(stringLocalizer["Appointment end date/time must be specified in UTC."]);
                else if (appointment.StartDateTimeUTC == appointment.EndDateTimeUTC)
                    bag.AddValidation(stringLocalizer["Appointment cannot have same start and end date/time."]);
                else if (appointment.StartDateTimeUTC > appointment.EndDateTimeUTC)
                    bag.AddValidation(stringLocalizer["Appointment start date/time cannot be after appointment end date/time."]);
                else if (appointment.EndDateTimeUTC - appointment.StartDateTimeUTC < Constants.MinimumAppoingmentDuration)
                    bag.AddValidation(stringLocalizer["Appointment duration cannot be shorter than {0} minutes.", Constants.MinimumAppoingmentDuration.Minutes.ToString()]);
                else if (appointment.EndDateTimeUTC - appointment.StartDateTimeUTC > Constants.MaximumAppointmentDuration)
                    bag.AddValidation(stringLocalizer["Appointment duration cannot be longer than {0} days.", Constants.MaximumAppointmentDuration.Days.ToString()]);
                else
                {
                    var others = application.Appointments.Where(x => x != appointment);
                    if (others.Any(y => y.IsOverlapping(appointment.StartDateTimeUTC, appointment.EndDateTimeUTC)))
                        bag.AddValidation(stringLocalizer["Appointments cannot overlap."]);
                }
            }
        }

        if (
                (current != null && current.Rejection != null && current.Rejection != application.Rejection)
                || (current == null && application.Rejection != null && application.Rejection != Rejection.Empty)
            )
            bag.AddValidation(stringLocalizer["Rejection fields are read-only and cannot be modified."]);
        if (
                (current != null && current.Acceptance != null && current.Acceptance != application.Acceptance)
                || (current == null && application.Acceptance != null && application.Acceptance != Acceptance.Empty)
            )
            bag.AddValidation(stringLocalizer["Acceptance fields are read-only and cannot be modified."]);

        return bag;
    }

    private void ResetNotificationsOnShiftedEvents(DateTime now, Entities.Application update, Entities.Application current)
    {
        var appointments = current.Appointments.ToDictionary(x => x.Id, x => x);
        foreach(var appointment in update.Appointments.Where(x => x.EmailNotificationSent || x.BrowserNotificationSent))
        {
            if (now > appointment.StartDateTimeUTC)
                continue;
            else {
                var match = appointments.ContainsKey(appointment.Id) ? appointments[appointment.Id] : null;
                if (match is null)
                    continue;

                if (match.StartDateTimeUTC != appointment.StartDateTimeUTC)
                {
                    appointment.EmailNotificationSent = false;
                    appointment.BrowserNotificationSent = false;
                }
            }
        }
    }
}

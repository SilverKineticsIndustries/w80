using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Common.Configuration;

namespace SilverKinetics.w80.Domain.Services.Application;

public class ApplicationAlertsService(
    IDateTimeProvider dateTimeProvider,
    IUserRepository userRepo,
    IConfiguration configuration,
    IApplicationRepository applicationRepo,
    IEmailMessageGenerator emailMessageGenerator,
    IEmailNotificationService emailNotificationService,
    ILogger<ApplicationAlertsService> logger)
    : IApplicationAlertsService
{
    public async Task<IDictionary<ObjectId, List<Guid>>> SendScheduleEmailAlertsAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeProvider.GetUtcNow();

        var emailsToSendOut = new List<EmailNotificationMessage>();
        var evntsToPersist = new Dictionary<ObjectId, List<Guid>>();
        var threshold = TimeSpan.FromMinutes(Convert.ToInt32(configuration[Keys.EmailAlertThresholdInMinutes]));

        foreach(var user in await userRepo.GetManyAsync(x =>
            x.DeactivatedUTC == null && x.EnableAppointmentEmailNotifications == true,
            cancellationToken))
        {
            foreach(var application in await applicationRepo.GetManyAsync(x => x.UserId == user.Id, cancellationToken).ConfigureAwait(false))
            {
                var appointments = application.GetScheduleEmailAlertsToSendOut(now, threshold);
                foreach(var appointment in appointments)
                {
                    try
                    {
                        var minutes = appointment.MinutesFromNowToStart(now);
                        var message = emailMessageGenerator.GetEmailScheduleAlertMessage(user, application.CompanyName, minutes);

                        await emailNotificationService.SendAsync([message], cancellationToken);

                        appointment.EmailNotificationSent = true;
                        if (!evntsToPersist.ContainsKey(application.Id))
                            evntsToPersist.Add(application.Id, []);

                        evntsToPersist[application.Id].Add(appointment.Id);

                    } catch (Exception e) {
                        logger.LogError(e,
                            "Error occured while sending email alert for application {applicationId}, appointment id {appointmentId}",
                            application.Id, appointment.Id);
                    }
                }
            }
        }

        return evntsToPersist;
    }
}
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SilverKinetics.w80.Application.Services;

public class ApplicationAlertsEmailBackgroundService(
    IConfiguration config,
    IServiceProvider serviceProvider)
    : IntervalBackgroundServiceBase(
        Convert.ToInt32(config[Keys.EmailNotificationsRunPeriodInSeconds])
    )
{
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (isRunning)
            return;

        isRunning = true;
        try
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IApplicationAlertsService>();
                var repo = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();

                var toUpdate = await service.SendScheduleEmailAlertsAsync(cancellationToken);
                if (!toUpdate.Any())
                    return;

                await repo.SetEmailNotificationSentOnAppoinmentsAsync(toUpdate, cancellationToken);
            }
        } finally {
            isRunning = false;
        }
    }

    private static bool isRunning = false;
}

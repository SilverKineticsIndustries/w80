using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Configuration;

namespace SilverKinetics.w80.Application.Services;

public class StatisticsRefreshBackgroundService(
    IConfiguration config,
    IServiceProvider serviceProvider)
    : IntervalBackgroundServiceBase(Convert.ToInt32(config[Keys.StatisticsRunPeriodInSeconds]))
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
                var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
                var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsGenerationService>();
                var systemStateRepo = scope.ServiceProvider.GetRequiredService<ISystemStateRepository>();
                var systemState = await systemStateRepo.GetSingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                await mongoClient.WrapInTransactionAsync(async
                (session) => {
                    var userStatistics = await statisticsService.UpdateStatisticsAsync(systemState, cancellationToken);
                    if (userStatistics.Any())
                    {
                        var statsRepo = scope.ServiceProvider.GetRequiredService<IStatisticsRepository>();
                        foreach(var stats in userStatistics)
                            await statsRepo.UpdateAsync(stats, cancellationToken);
                    }
                    await systemStateRepo.UpdateAsync(systemState, CancellationToken.None);
                },
                cancellationToken);
            }
        } finally {
            isRunning = false;
        }
    }

    private static bool isRunning = false;
}

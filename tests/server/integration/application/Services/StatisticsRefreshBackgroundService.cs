namespace SilverKinetics.w80.Application.IntegrationTests.Services;

[TestFixture(TestOf = typeof(Application.Services.StatisticsRefreshBackgroundService))]
public class StatisticsRefreshBackgroundService
{
    [Test]
    public async Task ExecuteAsync_rejectApplication_correctApplicationRejectionCountShouldBeUnderUserStatistics()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplicationUpdateRequestDto();
            var applicationService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await applicationService.UpsertAsync(app);
            await applicationService.RejectAsync(ObjectId.Parse(app.Id), ctx.CreateRejectionDto());

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var appId = ObjectId.Parse(app.Id);
            var application = await ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>().AsQueryable().FirstAsync(x => x.Id == appId);
            var stats = await ctx.Services.GetRequiredService<IStatisticsRepository>().GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            Assert.That(stats.ApplicationRejectionStateCounts[application.GetCurrentState().Id], Is.EqualTo(1));
        }
    }

    [Test]
    public async Task ExecuteAsync_rejectApplicationAndDeactivateIt_statisticsForApplicationRejectionCountShouldIncludeDeactivatedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplicationUpdateRequestDto();
            var applicationService = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var appId = ObjectId.Parse(app.Id);
            await applicationService.UpsertAsync(app);
            await applicationService.RejectAsync(appId, ctx.CreateRejectionDto());
            await applicationService.DeactivateAsync(appId);

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var application = await ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>().AsQueryable().FirstAsync(x => x.Id == appId);
            var stats = await ctx.Services.GetRequiredService<IStatisticsRepository>().GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            Assert.That(stats.ApplicationRejectionStateCounts[application.GetCurrentState().Id], Is.EqualTo(1));
        }
    }

    [Test]
    public async Task ExecuteAsync_rejectTwoApplication_correctApplicationRejectionCountShouldBeUnderUserStatistics()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var applicationService = ctx.Services.GetRequiredService<IApplicationApplicationService>();

            var app1 = ctx.CreateApplicationUpdateRequestDto();
            var app2 = ctx.CreateApplicationUpdateRequestDto();
            await applicationService.UpsertAsync(app1);
            await applicationService.UpsertAsync(app2);
            await applicationService.RejectAsync(ObjectId.Parse(app1.Id), ctx.CreateRejectionDto());
            await applicationService.RejectAsync(ObjectId.Parse(app2.Id), ctx.CreateRejectionDto());

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var appId = ObjectId.Parse(app1.Id);
            var application = await ctx.Services.GetRequiredService<IMongoCollection<Domain.Entities.Application>>().AsQueryable().FirstAsync(x => x.Id == appId);
            var stats = await ctx.Services.GetRequiredService<IStatisticsRepository>().GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            Assert.That(stats.ApplicationRejectionStateCounts[application.GetCurrentState().Id], Is.EqualTo(2));
        }
    }

    [Test]
    public async Task ExecuteAsync_generateData_systemStateLastStatisticsUTCShouldBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var collection = ctx.Services.GetRequiredService<IMongoCollection<SystemState>>();
            var preExecution = await collection.AsQueryable().FirstAsync();

            var app = ctx.CreateApplicationUpdateRequestDto();
            var applicationService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await applicationService.UpsertAsync(app);
            await applicationService.RejectAsync(ObjectId.Parse(app.Id), ctx.CreateRejectionDto());

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var postExecution = await collection.AsQueryable().FirstAsync();

            Assert.Multiple(() =>
            {
                Assert.That(preExecution.LastStatisticsRunUTC, Is.Not.EqualTo(postExecution.LastStatisticsRunUTC));
                Assert.That(postExecution.LastStatisticsRunUTC, Is.EqualTo(DateTime.UtcNow).Within(5).Seconds);
            });
        }
    }

    [Test]
    public async Task ExecuteAsync_executeStatisticsWithoutNewData_systemStateLastStatisticsUTCShouldStillBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var systemStateRepo = ctx.Services.GetRequiredService<ISystemStateRepository>();
            var preExecution = await systemStateRepo.GetSingleOrDefaultAsync(CancellationToken.None);

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var postExecution = await systemStateRepo.GetSingleOrDefaultAsync(CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(preExecution.LastStatisticsRunUTC, Is.Not.EqualTo(postExecution.LastStatisticsRunUTC));
                Assert.That(postExecution.LastStatisticsRunUTC, Is.EqualTo(DateTime.UtcNow).Within(5).Seconds);
            });
        }
    }

    [Test]
    public async Task ExecuteAsync_generateDataWhileSystemStateLastStatisticsRunUTCIsInTheFuture_noStatisticsShouldBeGenerated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplicationUpdateRequestDto();
            var applicationService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await applicationService.UpsertAsync(app);
            await applicationService.RejectAsync(ObjectId.Parse(app.Id), ctx.CreateRejectionDto());


            var systemStateRepo = ctx.Services.GetRequiredService<ISystemStateRepository>();
            var systemState = await systemStateRepo.GetSingleOrDefaultAsync(CancellationToken.None);
            systemState.LastStatisticsRunUTC = DateTime.UtcNow.AddDays(2);
            await systemStateRepo.UpdateAsync(systemState, CancellationToken.None);

            var statisticsService = ctx.Services.GetHostedService<Application.Services.StatisticsRefreshBackgroundService>();
            await statisticsService.ExecuteAsync(CancellationToken.None);

            var stats = await ctx.Services.GetRequiredService<IStatisticsRepository>().GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            Assert.That(stats, Is.Null);
        }
    }
}
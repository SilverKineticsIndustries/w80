namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationDeactivationService))]
public class ApplicationDeactivationService
{
    [Test]
    public async Task ValidateForArchiveAsync_rejectedApplicationBeingArchived_cannotArchiveRejectedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Reject(new Rejection(RejectionMethod.Email, "Some reason"));
            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            var bag = await service.ValidateForArchiveAsync(app);
            Assert.That(bag.Any(x => x.Message == "Rejected applications cannot be archived."));
        }
    }

    [Test]
    public async Task ValidateForReactivationAsync_notArchivedApplicationBeingUnarchived_applicationMustBeArchivedToBeUnarchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            var service = ctx.Services.GetRequiredService<IApplicationDeactivationService>();
            var bag = await service.ValidateForReactivationAsync(app);
            Assert.That(bag.Any(x => x.Message == "Application cannot be reactivated because it is not in a deactivated state."));
        }
    }

    [Test]
    public async Task ValidateForDeactivationAsync_notArchivedApplicationBeingUnarchived_applicationMustBeArchivedToBeUnarchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());
            var service = ctx.Services.GetRequiredService<IApplicationDeactivationService>();
            var bag = await service.ValidateForDeactivationAsync(app);
            Assert.That(bag.Any(x => x.Message == "Application cannot be deactivated because it is already in a deactivated state."));
        }
    }

    [Test]
    public async Task Deactivate_deactivateApplication_calendarEventsForApplicationShouldBeCleared()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(2),
                "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationDeactivationService>();
            service.Deactivate(app);
            Assert.That(app.Appointments.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task Deactivate_deactivateApplication_deactivationDateShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationDeactivationService>();
            service.Deactivate(app);
            Assert.That(app.DeactivatedUTC, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
        }
    }
}
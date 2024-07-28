namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationArchiveService))]
public class ApplicationArchiveService
{
    [Test]
    public async Task ValidateForArchiveAsync_deactivatedApplicationBeingArchived_cannotArchiveDeactivatedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());

            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            var bag = await service.ValidateForArchiveAsync(app);
            Assert.That(bag.Any(x => x.Message == "Deactivated applications cannot be archived."));
        }
    }

    [Test]
    public async Task ValidateForArchiveAsync_rejectedApplicationBeingArchived_cannotArchiveRejectedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Reject(ctx.CreateRejection());

            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            var bag = await service.ValidateForArchiveAsync(app);
            Assert.That(bag.Any(x => x.Message == "Rejected applications cannot be archived."));
        }
    }

    [Test]
    public async Task ValidateForUnarchiveAsync_notArchivedApplicationBeingUnarchived_applicationMustBeArchivedToBeUnarchived()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            var bag = await service.ValidateForUnarchiveAsync(app);
            Assert.That(bag.Any(x => x.Message == "Only archived applications can be unarchived."));
        }
    }

    [Test]
    public async Task Archive_archiveApplication_calendarEventsForApplicationShouldBeCleared()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(2),
                "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            service.Archive(app);
            Assert.That(app.Appointments.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task Archive_archiveApplication_archiveDateShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationArchiveService>();
            service.Archive(app);
            Assert.That(app.ArchivedUTC, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
        }
    }
}
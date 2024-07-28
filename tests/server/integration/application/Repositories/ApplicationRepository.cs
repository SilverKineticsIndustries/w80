namespace SilverKinetics.w80.Application.IntegrationTests.Repositories;

[TestFixture(TestOf = typeof(Application.Repositories.ApplicationRepository))]
public class ApplicationRepository
{
    [Test]
    public async Task SetEmailNotificationSentOnCalendarEventsAsync_twoCalendarEventsOneWithEmailNotificationsSentTrue_shouldPersistCorrectly()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var app = ctx.CreateApplication();
            app.UserId = ctx.GetCurrentUserId();

            app.Appointments.Add(ctx.CreateAppointment(
                now.AddMinutes(60),
                now.AddMinutes(60 * 2),
                "Test",
                id1));

            app.Appointments.Add(ctx.CreateAppointment(
                now.AddDays(1).AddMinutes(60),
                now.AddDays(1).AddMinutes(60 * 2),
                "Test 2",
                id2));

            var appService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await appService.UpsertAsync(ApplicationMapper.ToUpdateDTO(app), CancellationToken.None);

            var appRepo = ctx.Services.GetRequiredService<IApplicationRepository>();

            var application = await appRepo.FirstAsync(x => x.Id == app.Id, CancellationToken.None);
            Assume.That(application.Appointments.Count(), Is.EqualTo(2));
            application.Appointments.First(x => x.Id == id2).EmailNotificationSent = true;
            var toUpdate = new Dictionary<ObjectId, List<Guid>>();
            toUpdate.Add(app.Id, [application.Appointments.First(x => x.Id == id2).Id]);

            await appRepo.SetEmailNotificationSentOnAppoinmentsAsync(toUpdate, CancellationToken.None);

            var saved = await appRepo.FirstOrDefaultAsync(x => x.Id == app.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(saved?.Appointments.Where(x => x.Id == id1).First().EmailNotificationSent, Is.False);
                Assert.That(saved?.Appointments.Where(x => x.Id == id2).First().EmailNotificationSent, Is.True);
            });
        }
    }

    [Test]
    public async Task SetBrowserNotificationSentOnCalendarEventsAsync_twoCalendarEventsOneWithBrowserNotificationsSentTrue_shouldPersistCorrectly()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var repo = ctx.Services.GetRequiredService<IApplicationRepository>();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var app = ctx.CreateApplication();
            app.UserId = ctx.GetCurrentUserId();

            app.Appointments.Add(ctx.CreateAppointment(
                now.AddMinutes(60),
                now.AddMinutes(60 * 2),
                "Test",
                id1));
            app.Appointments.Add(ctx.CreateAppointment(
                now.AddDays(1).AddMinutes(60),
                now.AddDays(1).AddMinutes(60 * 2),
                "Test 2",
                id2));

            var appService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await appService.UpsertAsync(ApplicationMapper.ToUpdateDTO(app), CancellationToken.None);

            var appRepo = ctx.Services.GetRequiredService<IApplicationRepository>();

            var application = await appRepo.FirstAsync(x => x.Id == app.Id, CancellationToken.None);
            Assume.That(application.Appointments.Count(), Is.EqualTo(2));
            application.Appointments.First(x => x.Id == id2).BrowserNotificationSent = true;
            var toUpdate = new Dictionary<ObjectId, List<Guid>>();
            toUpdate.Add(app.Id, [application.Appointments.First(x => x.Id == id2).Id]);

            await appRepo.SetBrowserNotificationSentOnAppointmentsAsync(toUpdate, CancellationToken.None);

            var saved = await appRepo.FirstOrDefaultAsync(x => x.Id == app.Id, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.That(saved?.Appointments.Where(x => x.Id == id1).First().BrowserNotificationSent, Is.False);
                Assert.That(saved?.Appointments.Where(x => x.Id == id2).First().BrowserNotificationSent, Is.True);
            });
        }
    }
}
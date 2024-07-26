using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationAlertsService))]
public class ApplicationAlertsService
{
    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentBeforeAlertThreshold_emailAlertShouldNotBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes]) + 10),
                now.AddHours(8));

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(0));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(0));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentAtBorderOfAlertThreshold_emailAlertShouldBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(1));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentAtBorderOfAlertThreshold_correctEmailNotificationShouldBeSent()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var stringLocalizer = ctx.Services.GetRequiredService<IStringLocalizer<Common.Resource.Resources>>();
            var user = await ctx.Services.GetRequiredService<IUserRepository>().GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            var email = emailSender.Emails.Value.First();
            var subject = stringLocalizer["Appointment Alert"].Value;

            Assert.Multiple(() =>
            {
                Assert.That(email.Subject, Is.EqualTo(subject));
                Assert.That(email.Addresses.Contains(user.Email), Is.True);
                Assert.That(email.Template, Is.EqualTo(TemplateType.EmailApplicationScheduleAlert));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentInsideAlertThreshold_emailAlertShouldBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes]) - 20),
                now.AddHours(8));

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(1));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentInsideAlertThreshold_appointmentShouldBeMarkedAsSent()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);
            await ctx.Services.GetRequiredService<IApplicationRepository>().SetEmailNotificationSentOnAppoinmentsAsync(update, CancellationToken.None);

            var updatedApp = await ctx.Services.GetRequiredService<IApplicationRepository>().GetSingleOrDefaultAsync(x => x.Id == app.Id, CancellationToken.None);
            Assert.That(updatedApp.Appointments.First().EmailNotificationSent, Is.EqualTo(true));
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentInsideAlertThresholdWithEmailAlertsTurnedOff_emailAlertShouldNotBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();
            await DisableEmailAlertsOnTestUserAsync(ctx);

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;
            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(0));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(0));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_deactivatedUserWithOneAppointmentInsideAlertThreshold_emailAlertShouldNotBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;

            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();
            var current = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
            user.Deactivate(DateTime.UtcNow);
            await userRepo.DeactivateAsync(user, CancellationToken.None);

            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(0));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(0));
            });
        }
    }

    [Test]
    public async Task SendScheduleEmailAlertsAsync_userWithOneAppointmentOnDeactivatedApplicationInsideAlertThreshold_emailAlertShouldNotBeCreated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            await EnableEmailAlertsOnTestUserAsync(ctx);

            var now = DateTime.UtcNow;
            var app = await CreateApplicationWithAppointmentAsync(
                ctx,
                ctx.GetCurrentUserId(),
                now.AddMinutes(Convert.ToInt32(ctx.Config()[Keys.EmailAlertThresholdInMinutes])),
                now.AddHours(8));

            var appService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
            await appService.DeactivateAsync(app.Id, CancellationToken.None);

            var service = ctx.Services.GetRequiredService<IApplicationAlertsService>();

            var emailSender = ctx.Services.GetRequiredService<IEmailSenderService>() as EmailSenderServiceFake;

            var userRepo = ctx.Services.GetRequiredService<IUserRepository>();
            var current = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);

            var update = await service.SendScheduleEmailAlertsAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(update.Count, Is.EqualTo(0));
                Assert.That(emailSender.Emails.Value.Count, Is.EqualTo(0));
            });
        }
    }

    private async Task<Domain.Entities.Application> CreateApplicationWithAppointmentAsync(
        TestHelper.TestContext ctx,
        ObjectId userId,
        DateTime eventStartDateTime,
        DateTime eventEndDateTime)
    {
        var app = ctx.CreateApplication();
        app.UserId = userId;
        app.Appointments.Add(new Appointment()
        {
            ApplicationStateId = app.GetCurrentState().Id.ToString(),
            StartDateTimeUTC = eventStartDateTime,
            EndDateTimeUTC = eventEndDateTime,
            Id = Guid.NewGuid(),
            Description = "Test"
        });

        var appService = ctx.Services.GetRequiredService<IApplicationApplicationService>();
        await appService.UpsertAsync(ApplicationMapper.ToUpdateDTO(app), CancellationToken.None);

        return app;
    }

    private async Task EnableEmailAlertsOnTestUserAsync(TestHelper.TestContext ctx)
    {
        var userRepo = ctx.Services.GetRequiredService<IUserRepository>();
        var current = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
        var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
        user.EnableEventEmailNotifications = true;
        await userRepo.UpsertAsync(user, current, CancellationToken.None);
    }

    private async Task DisableEmailAlertsOnTestUserAsync(TestHelper.TestContext ctx)
    {
        var userRepo = ctx.Services.GetRequiredService<IUserRepository>();
        var current = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
        var user = await userRepo.GetSingleOrDefaultAsync(x => x.Id == ctx.GetCurrentUserId(), CancellationToken.None);
        user.EnableEventEmailNotifications = false;
        await userRepo.UpsertAsync(user, current, CancellationToken.None);
    }
}

namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationRejectionService))]
public class ApplicationRejectionService
{
    [Test]
    public async Task ValidateAsync_noRejectionReasonProvided_rejectionReasonMustBeProvided()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var rejection = new Rejection() { Method = RejectionMethod.Email };

            var bag = await service.ValidateAsync(app, rejection);
            Assert.That(bag.Any(x => x.Message == "Rejection reason cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_noRejectionMethodProvided_rejectionMethodMustBeProvided()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var rejection = new Rejection() { Reason = "Some reason" };

            var bag = await service.ValidateAsync(app, rejection);
            Assert.That(bag.Any(x => x.Message == "Rejection method must be provided."));
        }
    }

    [Test]
    public async Task ValidateAsync_rejectionReasonIsLargerThanMaxLength_rejectionReasonMustBeSmallerThanMaxLength()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var rejection = new Rejection();
            rejection.Reason = string.Join("", Enumerable.Repeat("A", Rejection.ReasonMaxSize + 1));

            var bag = await service.ValidateAsync(app, rejection);
            Assert.That(bag.Any(x => x.Message == $"Rejection reason max length is {Rejection.ReasonMaxSize} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_rejectionResponseTextIsLargerThanMaxLength_rejectionResponseTextMustBeSmallerThanMaxLength()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var rejection = new Rejection();
            rejection.Reason = "Some reason";
            rejection.Method = RejectionMethod.Email;
            rejection.ResponseText = string.Join("", Enumerable.Repeat("A", Rejection.ResponseTextMaxSize + 1));

            var bag = await service.ValidateAsync(app, rejection);
            Assert.That(bag.Any(x => x.Message == $"Rejection response text max length is {Rejection.ResponseTextMaxSize} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_deactivatedApplicationBeingRejected_cannotRejectDeactivatedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Deactivate(ctx.Services.GetRequiredService<IDateTimeProvider>());

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var bag = await service.ValidateAsync(app, ctx.CreateRejection());
            Assert.That(bag.Any(x => x.Message == "Deactivated applications cannot be rejected."));
        }
    }

    [Test]
    public async Task ValidateAsync_archivedApplicationBeingRejected_cannotRejectArchivedApplication()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Archive(ctx.Services.GetRequiredService<IDateTimeProvider>());

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            var bag = await service.ValidateAsync(app, ctx.CreateRejection());
            Assert.That(bag.Any(x => x.Message == "Archived applications cannot be rejected."));
        }
    }

    [Test]
    public async Task Reject_rejectApplication_calendarEventsForApplicationShouldBeCleared()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(new Appointment()
            {
                StartDateTimeUTC = DateTime.UtcNow.AddDays(2),
                EndDateTimeUTC = DateTime.UtcNow.AddDays(2),
                Description = "Interview"
            });

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            service.Reject(app, ctx.CreateRejection());
            Assert.That(app.Appointments.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task Reject_rejectApplication_rejectionDateShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var service = ctx.Services.GetRequiredService<IApplicationRejectionService>();
            service.Reject(app, ctx.CreateRejection());
            Assert.That(app.Rejection.RejectedUTC, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
        }
    }
}
namespace SilverKinetics.w80.Domain.UnitTests.Entities;

[TestFixture(TestOf = typeof(Domain.Entities.Application))]
public class Application
{
    [Test]
    public async Task InitializeApplicationStates_initStatesOnNewApplication_statesShouldBePreloadedFromGlobalList()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var applStates = ctx.GetApplicationStates();
            var app = new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), applStates);
            Assert.That(app.States.Count == applStates.Count());
        }
    }

    [Test]
    public async Task InitializeApplicationStates_initStatesOnNewApplication_activeStateShouldBeSet()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var applStates = ctx.GetApplicationStates();
            var app = new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), applStates);

            var selected = app.States.SingleOrDefault(x => x.IsCurrent);
            Assert.That(selected?.Name, Is.EqualTo(applStates.OrderBy(x => x.SeqNo).First().Name));
        }
    }

    [Test]
    public async Task InitializeApplicationStates_passInEmptyApplicationStateList_exceptionShouldBeThrown()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("Parameter states must contain a list of all application states. (Parameter 'states')"
            ),
            () => new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), new List<ApplicationState>()));
        }
    }

    [Test]
    public async Task InitializeApplicationStates_firstItemIsDeactivedInThePassedInApplicationStateList_deactivatedItemShouldNotBeInitialized()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var applStates = ctx.GetApplicationStates().ToList();
            var deactivated = applStates.OrderBy(x => x.SeqNo).First();
            deactivated.DeactivatedUTC = DateTime.UtcNow;

            var app = new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), applStates);
            Assert.That(app.States.Any(x => x.Name == deactivated.Name), Is.False);
        }
    }

    [Test]
    public async Task InitializeApplicationStates_onlyOneActiveApplicationListState_thereShouldBeAtLeastTwoActiveApplicationStates()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var applStates = ctx.GetApplicationStates().ToList();
            applStates = applStates.Take(2).ToList();
            applStates.ForEach(x => x.DeactivatedUTC = DateTime.UtcNow);

            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("There must be at least two active application states. (Parameter 'states')"
            ),
            () => new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), applStates));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsOutsideOfAlertThreshold_shouldNotBeReturned()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                now.Add(threshold).AddMinutes(10),
                now.Add(threshold).AddMinutes(10).AddHours(4),
                "appointment one"));

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.Zero);
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsInsideOfAlertThreshold_shouldNotBeReturned()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                now.Add(threshold).AddMinutes(-1),
                now.Add(threshold).AddMinutes(10).AddHours(4),
                "appointment one"));

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichIsRightOnBorderOfAlertThreshold_shouldNotBeReturned()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                now.Add(threshold),
                now.Add(threshold).AddMinutes(10).AddHours(4),
                "appointment one"));

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventWhichAlreadyStarted_shouldNotBeReturned()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                now.Subtract(threshold),
                now.Subtract(threshold).AddMinutes(10).AddHours(4),
                "appointment one"));

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    public async Task GetScheduleEmailAlertsToSendOut_scheduledEventStartingAtThisExactMoment_shouldNotBeReturned()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                now,
                now.AddHours(4),
                "appointment one"));

            var alerts = app.GetScheduleEmailAlertsToSendOut(now, threshold);
            Assert.That(alerts.Count(), Is.EqualTo(0));
        }
    }
}

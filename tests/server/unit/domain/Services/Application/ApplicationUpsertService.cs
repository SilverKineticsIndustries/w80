namespace SilverKinetics.w80.Domain.UnitTests.Services.Application;

[TestFixture(TestOf = typeof(Domain.Services.Application.ApplicationUpsertService))]
public class ApplicationUpsertService
{
    [Test]
    public async Task ValidateAsync_noCompanyName_companyNameIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompanyName = "";
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Company name cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_companyNameLargerThanMax_companyNameCannotBeMoreThanMax()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompanyName = string.Join(string.Empty, Enumerable.Repeat('A', 500));
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == $"Company name max length is {Domain.Entities.Application.CompanyNameMaxLength} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMinEnteredCompensationMaxNotEnteredAndCompensationTypeNotEntered_compensationTypeIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMin = 100;
            app.CompensationType = null;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation type should be entered if minimum or maximum compensation is entered."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMaxEnteredCompensationMinNotEnteredAndCompensationTypeNotEntered_compensationTypeIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMax = 100;
            app.CompensationType = CompensationType.None;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation type should be entered if minimum or maximum compensation is entered."));
        }
    }

    [Test()]
    public async Task ValidateAsync_roleDescriptionNotEntered_roleDescriptionIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.RoleDescription = "";
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Role description cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_roleDescriptionGreaterThanMaxLength_roleDescriptionCannotBeMoreThanMax()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.RoleDescription = string.Join(string.Empty, Enumerable.Repeat('A', Domain.Entities.Application.RoleDescriptionMaxLength + 1));
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == $"Role description max length is {Domain.Entities.Application.RoleDescriptionMaxLength} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_roleGreaterThanMaxLength_roleCannotBeMoreThanMax()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Role = string.Join(string.Empty, Enumerable.Repeat('A', Domain.Entities.Application.RoleMaxLength + 1));
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == $"Role max length is {Domain.Entities.Application.RoleMaxLength} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMaxLessThenZero_compensationMaxCannotBeLessThanZero()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMax = -10;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation max cannot be a less than or equal to zero."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMaxIsZero_compensationMaxCannotBeZero()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMax = 0;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation max cannot be a less than or equal to zero."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMinLessThenZero_compesationMinCannotBeLessThanZero()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMin = -10;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation min cannot be a less than or equal to zero."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMinIsZero_compensationMinCannotBeZero()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMin = 0;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation min cannot be a less than or equal to zero."));
        }
    }

    [Test]
    public async Task ValidateAsync_compensationMinGreaterThanPayMax_compensationMinCannotBeGreaterThanPayMax()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.CompensationMin = 10;
            app.CompensationMax = 9;
            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Compensation min cannot be greater than compensation max."));
        }
    }

    [Test]
    public async Task ValidateAsync_UserIdNotEntered_userIdIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.UserId = default;

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "User cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_currentStateNotSelected_applicationShouldHaveCurrentState()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            foreach (var state in app.States)
                state.IsCurrent = false;

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "An application must be in a state."));
        }
    }

    [Test]
    public async Task ValidateAsync_multipleStatesSelected_applicationShouldOnlyBeInOneCurrentState()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            foreach (var state in app.States)
                state.IsCurrent = true;

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "An application cannot in more than one state at a time."));
        }
    }

    [Test]
    public async Task ValidateAsync_twoAppointmentsStartAtSameTime_twoAppointmentsCannotOverlap()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var eventStartDate = DateTime.UtcNow;
            var eventEndDate = DateTime.UtcNow.AddHours(4);
            app.Appointments.Add(ctx.CreateAppointment(eventStartDate, eventEndDate, "Interview 1"));
            app.Appointments.Add(ctx.CreateAppointment(eventStartDate, eventEndDate, "Interview 2"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointments cannot overlap."));
        }
    }

    [Test]
    public async Task ValidateAsync_twoAppointmentsOverlap_appointmentsCannotOverlap()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var event1StartDate = now;
            var event1EndDate = now.AddHours(4);
            var event2StartDate = now.AddHours(2);
            var event2EndDate = now.AddHours(6);
            app.Appointments.Add(ctx.CreateAppointment(event1StartDate, event1EndDate, "Interview 1"));
            app.Appointments.Add(ctx.CreateAppointment(event2StartDate, event2EndDate, "Interview 2"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointments cannot overlap."));
        }
    }

    [Test]
    public async Task ValidateAsync_sameStartAndEndDateTime_cannotHaveSameStartAndEndDateTime()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            app.Appointments.Add(ctx.CreateAppointment(now, now, "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment cannot have same start and end date/time."));
        }
    }

    [Test]
    public async Task ValidateAsync_startAndEndSmallerThanAllowedMinimum_cannotHaveStartAndEndDateTimeSmallerThanMinimum()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var startDateTime = now;
            var endDateTime = now.Add(Constants.MinimumAppoingmentDuration).AddSeconds(-1);
            app.Appointments.Add(ctx.CreateAppointment(startDateTime, endDateTime, "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment duration cannot be shorter than " + Constants.MinimumAppoingmentDuration.Minutes.ToString() + " minutes."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentStartDateTimeIsAfterEndDateTime_cannotHaveStartDateTimeBeAfterEndDateTime()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var startDateTime = now;
            var endDateTime = now.AddHours(-4);
            app.Appointments.Add(ctx.CreateAppointment(startDateTime, endDateTime, "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment start date/time cannot be after appointment end date/time."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentDescriptionIsEmpty_descriptionCannotBeEmpty()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            app.Appointments.Add(ctx.CreateAppointment(now, now.AddHours(-4), string.Empty));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment description cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentDescriptionIsGreaterThanMax_descriptionCannotBeGreaterThanMax()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            app.Appointments.Add(ctx.CreateAppointment(
                now,
                now.AddHours(-4),
                string.Join(string.Empty, Enumerable.Repeat("A", Appointment.DescriptionMaxLength + 1))));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == $"Appointment description max length is {Appointment.DescriptionMaxLength} characters."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentStartDateIsMissing_appointmentStartDateTimeIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                default,
                DateTime.UtcNow,
                "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment start date/time cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentEndDateIsMissing_appointmentEndDateTimeIsRequired()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Appointments.Add(ctx.CreateAppointment(
                DateTime.UtcNow,
                default,
                "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment end date/time cannot be empty."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentIsLongerThanMaximumLength_cannotHaveStartAndEndDateTimeLargerThanMaximum()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var startDateTime = now;
            var endDateTime = now.Add(Constants.MaximumAppointmentDuration).AddSeconds(1);

            app.Appointments.Add(ctx.CreateAppointment(
                startDateTime,
                endDateTime,
                "Interview"));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointment duration cannot be longer than " + Constants.MaximumAppointmentDuration.Days.ToString() + " days."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentHasDuplicateObjectId_appointmentCannotHaveDuplicateObjectIds()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var startDateTime = now;
            var endDateTime = now.AddHours(4);
            var objId = Guid.NewGuid();

            app.Appointments.Add(ctx.CreateAppointment(
                startDateTime,
                endDateTime,
                "Interview",
                objId));

            app.Appointments.Add(ctx.CreateAppointment(
                startDateTime.AddDays(1),
                endDateTime.AddDays(1),
                "Interview",
                objId));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointments must have unique Ids."));
        }
    }

    [Test]
    public async Task ValidateAsync_appointmentHasEmptyObjectId_appointmentMustHaveValidObjectIds()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();

            var now = DateTime.UtcNow;
            var startDateTime = now;
            var endDateTime = now.AddHours(4);
            app.Appointments.Add(ctx.CreateAppointment(
                startDateTime,
                endDateTime,
                "Interview",
                Guid.Empty));

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Appointments must have valid Ids."));
        }
    }

    [Test]
    public async Task UpsertAsync_updateRejectionFields_rejectionFieldsAreReadOnlyAndCantBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Rejection = new Rejection(RejectionMethod.Email, string.Empty);

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Rejection fields are read-only and cannot be modified."));
        }
    }

    [Test]
    public async Task UpsertAsync_updateAcceptanceFields_acceptanceFieldsAreReadOnlyAndCantBeUpdated()
    {
        using (var ctx = await TestContextFactory.Create().SeedDatabaseAsync())
        {
            var app = ctx.CreateApplication();
            app.Acceptance= new Acceptance(AcceptanceMethod.Email);

            var service = ctx.Services.GetRequiredService<IApplicationUpsertService>();
            var bag = await service.ValidateAsync(app);

            Assert.That(bag.Any(x => x.Message == "Acceptance fields are read-only and cannot be modified."));
        }
    }
}

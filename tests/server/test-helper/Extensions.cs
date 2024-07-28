using Microsoft.Extensions.Hosting;

namespace SilverKinetics.w80.TestHelper;

public static class Extensions
{
    public static User CreateUser(this TestContext cxt, string? email = null)
    {
        return new User(ObjectId.GenerateNewId(), Role.User, email ?? "testuser@silverkinetics.dev")
        {
            Culture = SupportedCultures.DefaultCulture,
            Nickname = "testuser@silverkientics.dev",
            TimeZone = SupportedTimeZones.DefaultTimezone
        };
    }

    public static UserUpsertRequestDto CreateUserUpsertDto(this TestContext cxt, string? email = null)
    {
        return
            new UserUpsertRequestDto() {
                Nickname = email,
                Email = email,
                Culture = SupportedCultures.DefaultCulture,
                TimeZone = SupportedTimeZones.DefaultTimezone,
                Role = Role.User,
                Id = ObjectId.GenerateNewId().ToString()
            };
    }

    public static Domain.Entities.Application CreateApplication(this TestContext ctx)
    {
        return
            new Domain.Entities.Application(ObjectId.GenerateNewId(), ctx.GetTestUserID(), _GetApplicationStates())
            {
                CompanyName = "Company 1",
                Role = "Senior Java Developer",
                RoleDescription = "Senior Java Developer. Must know Java.",
                CompensationMax = 140000,
                CompensationMin = 120000,
                CompensationType = CompensationType.Salary,
                PositionType = PositionType.Fulltime,
                WorkSetting = WorkSetting.OnSite,
                Industry = "Healthcare",
                HQLocation = "Miami",
                PositionLocation = "Miami",
                TravelRequirements = "No travel is required",
            };
    }

    public static ApplicationUpdateRequestDto CreateApplicationUpdateRequestDto(this TestContext ctx)
    {
        return ApplicationMapper.ToUpdateDTO(CreateApplication(ctx));
    }

    public static InvitationProcessRequestDto CreateInvitationProcessRequestDto(this TestContext ctx, string email, string code, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(password))
            password = ctx.GetTestUserPassword();

        return new InvitationProcessRequestDto() {
            Email = email,
            InvitationCode = code,
            Password = password
        };
    }

    public static RejectionDto CreateRejectionDto(this TestContext ctx)
    {
        var rejection = new RejectionDto();
        rejection.Reason = "Some reason";
        rejection.Method = RejectionMethod.Email;
        return rejection;
    }
    public static Rejection CreateRejection(this TestContext ctx)
    {
        return new Rejection(RejectionMethod.Email, "Some reason");
    }

    public static AppointmentDto CreateAppointmentDto(this TestContext ctx, DateTime startEventDateTime, DateTime endEventDateTime)
    {
        var appointment = new AppointmentDto();
        appointment.Id = Guid.NewGuid().ToString();
        appointment.StartDateTimeUTC = startEventDateTime;
        appointment.EndDateTimeUTC = endEventDateTime;
        appointment.Description = Guid.NewGuid().ToString();
        return appointment;
    }

    public static ContactDto CreateContactDto(this TestContext ctx)
    {
        var contact = new ContactDto();
        contact.SeqNo = 1;
        contact.Type = ContactType.Email;
        contact.ContactName = "name";
        contact.ContactParameter = ctx.GetTestUserEmail();
        contact.Role = ContactRole.HumanResources;
        return contact;
    }

    public static IList<ApplicationState> GetApplicationStates(this TestContext ctx)
    {
        return _GetApplicationStates();
    }

    public async static Task<ComplexResponseDto<UserViewDto>> UpsertNewUserAsync(this TestContext ctx,
        string email, string? id = null, bool enableEventEmailNotifications = false)
    {
        ctx.SetUserToAdmin();
        var userId = id ?? ObjectId.GenerateNewId().ObjectIdToStringId();
        var service = ctx.Services.GetRequiredService<IUserApplicationService>();
        var ret =
            await service.UpsertAsync(
                new UserUpsertRequestDto() {
                    EnableEventEmailNotifications = enableEventEmailNotifications,
                    Nickname = email,
                    Email = email,
                    Culture = SupportedCultures.DefaultCulture,
                    TimeZone = SupportedTimeZones.DefaultTimezone,
                    Role = Role.User,
                    Id = userId
                },
                RequestSourceInfo.Empty,
                CancellationToken.None
            );
        ctx.SetUserToUser();
        return ret;
    }

    public async static Task<InvitationProcessResponseDto> LoginUserWithInvitationCodeAsync(this TestContext ctx, string email, string invitationCode)
    {
        var invitationProcessRequest = ctx.CreateInvitationProcessRequestDto(email, invitationCode);
        var authenticationService = ctx.Services.GetRequiredService<IAuthenticationApplicationService>();
        return await authenticationService.ProcessInvitationAsync(invitationProcessRequest, CancellationToken.None);
    }

    public async static Task DeactivateUserAsync(this TestContext ctx, ObjectId userId)
    {
        ctx.SetUserToAdmin();
        var userService = ctx.Services.GetRequiredService<IUserApplicationService>();
        await userService.DeactivateAsync(userId, RequestSourceInfo.Empty, CancellationToken.None);
    }

    public static T GetHostedService<T>(this IServiceProvider serviceProvider)
        where T : class, IHostedService
    {
        return
            serviceProvider.GetServices<IHostedService>().OfType<T>().Single();
    }

    public static Appointment CreateAppointment(this TestContext ctx, DateTime start, DateTime end, string description = "description", Guid? id = null)
    {
        return
            new Appointment(
                id ?? Guid.NewGuid(),
                description,
                start,
                end);
    }

    private static IList<ApplicationState> _GetApplicationStates()
    {
        return
            new List<ApplicationState>()
            {
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecad42dc928266934b5d5"),
                        "Applied",
                        "007BFF",
                        0
                    )
                },
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecb1d2dc928266934b5d6"),
                        "Client Responded",
                        "00A3FF",
                        10
                    )
                },
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecb412dc928266934b5d7"),
                        "Screening",
                        "00D2FF",
                        20
                    )
                },
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecb592dc928266934b5d8"),
                        "Assessments",
                        "00FFD1",
                        30
                    )
                },
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecb902dc928266934b5d9"),
                        "Hiring Manager Screening",
                        "00FF85",
                        40
                    )
                },
                {
                    new ApplicationState(
                        ObjectId.Parse("661ecba72dc928266934b5da"),
                        "Decision / Job Offer",
                        "9EFF00",
                        50
                    )
                }
            };
    }
}
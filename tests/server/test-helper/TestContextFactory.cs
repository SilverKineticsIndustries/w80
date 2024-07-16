using Microsoft.Extensions.Logging;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.TestHelper;

public static class TestContextFactory
{
    public static TestContext Create()
    {
        var databaseName = Guid.NewGuid().ToString().Replace("-", string.Empty);
        return new TestContext(Provider(databaseName), databaseName);
    }

    private static ServiceProvider Provider(string databaseName)
    {
        var services = new ServiceCollection();
        services.AddLocalization();
        services.AddDependencies(TestingConfig.Value, databaseName: databaseName);

        services.AddScoped<IEmailSenderService, EmailSenderServiceFake>();
        services.AddScoped<ICookieManager, CookieManagerFake>();
        services.AddScoped<ISecurityContext, SecurityContextFake>();
        services.AddScoped<IConfiguration>((services) => { return TestingConfig.Value; });

        // IStringLocalizer requires some kind of logging, so that is why we add this here ..
        services.AddLogging((builder) => { builder.ClearProviders(); });
        return services.BuildServiceProvider();
    }

    private static readonly Lazy<ConfigurationManager> TestingConfig = new(() =>
    {
        var configuration = new ConfigurationManager();
        configuration.AddEnvironmentVariables(Constants.EnvironmentVariablePrefix);
        configuration.AddInMemoryCollection
        (
            new Dictionary<string,string?>()
            {
                { Keys.Secrets.JwtKey, Guid.NewGuid().ToString() },
                { Keys.Secrets.InvitationKey, Guid.NewGuid().ToString() },
                { Keys.Secrets.EmailConfirmationKey, Guid.NewGuid().ToString() },
                { Keys.JwtAccessLifetimeInMinutes, "10" },
                { Keys.JwtRefreshLifetimeInDays, "1" },
                { Keys.InvitationCodeLifetimeInHours, (24*7).ToString() },
                { Keys.EmailConfirmationLifetimeInHours, "24" },
                { Keys.EmailAlertThresholdInMinutes, "30" },
                { Keys.PasswordMinimumLength, "14" },
                { Keys.Appname, "Tests" },
                { Keys.Domain, "TestsDomain" },
            }
        );
        return configuration;
    });
}

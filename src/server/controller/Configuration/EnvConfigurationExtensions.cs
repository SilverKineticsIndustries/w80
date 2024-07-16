namespace SilverKinetics.w80.Controller.Configuration;

public static class EnvConfigurationExtensions
{
    public static IConfigurationBuilder AddEnvFile(this IConfigurationBuilder builder)
        => builder.AddEnvFile(".env", true);

    public static IConfigurationBuilder AddEnvFile(
        this IConfigurationBuilder builder,
        string dotEnvFile,
        bool optional
    )
    {
        builder.Add<EnvConfigurationSource>(s =>
        {
            s.DotEnvFile = dotEnvFile;
            s.Optional = optional;
        });
        return builder;
    }
}
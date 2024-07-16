namespace SilverKinetics.w80.Controller.Configuration;

public class EnvConfigurationSource : IConfigurationSource
{
    public string DotEnvFile { get; set; } = ".env";
    public bool Optional { get; set; } = false;

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new EnvConfigurationProvider(DotEnvFile, Optional);
}

namespace SilverKinetics.w80.Controller.Configuration;

public class EnvConfigurationProvider(string dotEnvFile, bool optional = false) : ConfigurationProvider
{
    private readonly string _dotEnvFile = dotEnvFile;
    private readonly bool _optional = optional;

    public override void Load()
    {
        try
        {
            foreach (var ln in File.ReadAllLines(_dotEnvFile)
                                   .Where(x =>
                                        x.StartsWith(Common.Constants.EnvironmentVariablePrefix)
                                        || x.StartsWith("ASPNETCORE")
                                    ))
            {
                var line = ln.Trim();

                if (line.StartsWith('#')) continue;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var eqIx = line.IndexOf('=');
                if (eqIx < 0) continue;

                var key = line[..eqIx];
                var value = line[(eqIx + 1)..];
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (value == null) continue;

                key = key.Replace("__", ":");
                if (key.StartsWith(Common.Constants.EnvironmentVariablePrefix))
                    key = key.Replace(Common.Constants.EnvironmentVariablePrefix, string.Empty);

                base.Set(key, value);
            }
        }
        catch(FileNotFoundException)
        {
            if (_optional) return;
            throw;
        }
    }
}

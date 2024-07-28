using System.Reflection;

namespace SilverKinetics.w80.Common.Resource;

public class Resources
{
    public static async Task<string> GetEmbeddedResourceFileAsync(Assembly assembly, string filename)
    {
        using (var stream = assembly.GetManifestResourceStream(filename))
        {
            if (stream == null)
                throw new Exception($"Unable to load manifest resource steam for {filename} from {assembly.FullName}");

            using (var reader = new StreamReader(stream))
            {
                string result = await reader.ReadToEndAsync();
                return result;
            }
        }
    }
}

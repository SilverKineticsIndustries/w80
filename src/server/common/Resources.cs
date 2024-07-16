using System.Reflection;

namespace SilverKinetics.w80.Common;

public static class Resources
{
    public static async Task<string> GetEmbeddedResourceFileAsync(Assembly assembly, string filename)
    {
        using (var s = assembly.GetManifestResourceStream(filename))
        {
            using (var r = new System.IO.StreamReader(s))
            {
                string result = await r.ReadToEndAsync();
                return result;
            }
        }
    }
}

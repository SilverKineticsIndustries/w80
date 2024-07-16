using System.Globalization;
using System.Collections.ObjectModel;

namespace SilverKinetics.w80.Common;

public static class SupportedCultures
{
    public const string DefaultCulture = "en-US";

    public static IReadOnlyDictionary<string, CultureInfo> Cultures { get { return _supportingCultures; }}

    // TODO: Read about frozen dictionaries for these (they are suppose to have fast read times)
    private static readonly IReadOnlyDictionary<string, CultureInfo> _supportingCultures
        = new ReadOnlyDictionary<string, CultureInfo>(
            new Dictionary<string, CultureInfo>()
            {
                { "en-US", CultureInfo.GetCultureInfo("en-US") },
                { "de-DE", CultureInfo.GetCultureInfo("de-DE") },
            }
        );
}
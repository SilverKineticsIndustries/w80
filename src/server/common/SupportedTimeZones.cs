
using System.Collections.ObjectModel;

namespace SilverKinetics.w80.Common;

public static class SupportedTimeZones
{
    public const string DefaultTimezone = "America/New_York";

    public static IReadOnlyDictionary<string, TimeZoneInfo> TimeZones { get { return _supportingTimezones; }}

    // TODO: Read about frozen dictionaries for these (they are suppose to have fast read times)
    private static readonly IReadOnlyDictionary<string, TimeZoneInfo> _supportingTimezones
        = new ReadOnlyDictionary<string, TimeZoneInfo>(
            new Dictionary<string, TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones().ToDictionary(x => x.Id, x => x))
        );
}
namespace SilverKinetics.w80.Common;

public sealed class Constants
{
    public const string EnvironmentVariablePrefix= "W80_";
    public const string DefaultTimeZone = "America/New_York";
    public readonly static TimeSpan MinimumAppoingmentDuration = new(hours: 0, minutes: 5, seconds: 0);
    public readonly static TimeSpan MaximumAppointmentDuration = new(days: 7, hours: 0, minutes: 0, seconds: 0);
}
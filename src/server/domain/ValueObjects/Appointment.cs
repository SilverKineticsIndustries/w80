using System.ComponentModel.DataAnnotations;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Appointment
{
    public Guid Id { get; set;}
    public DateTime StartDateTimeUTC { get; set; }
    public DateTime EndDateTimeUTC { get; set; }

    [MaxLength(DescriptionMaxLength)]
    public string Description { get; set; }
    public string? ApplicationStateId { get; set; }

    public bool BrowserNotificationSent { get; set; }
    public bool EmailNotificationSent { get; set; }

    public bool IsNowWithinThresholdOfEventStart(DateTime utcNow, TimeSpan threshold)
    {
        var minutesToStart = MinutesFromNowToStart(utcNow);
        if (minutesToStart <= 0)
            return false;

        return TimeSpan.FromMinutes(minutesToStart) <= threshold;
    }

    public int MinutesFromNowToStart(DateTime utcNow)
    {
        var span = StartDateTimeUTC - utcNow;
        return (int)double.Truncate(span.TotalMinutes);
    }

    public bool IsOverlapping(DateTime startDateTime, DateTime endDateTime)
    {
        if (startDateTime >= StartDateTimeUTC && startDateTime <= EndDateTimeUTC)
            return true;

        if (endDateTime >= StartDateTimeUTC && endDateTime <= EndDateTimeUTC)
            return true;

        if (endDateTime >= StartDateTimeUTC && endDateTime <= EndDateTimeUTC)
            return true;

        if (startDateTime <= StartDateTimeUTC && endDateTime >= EndDateTimeUTC)
            return true;

        return false;
    }

    public const int DescriptionMaxLength = 200;
}
namespace SilverKinetics.w80.Application.DTOs;

public record AppointmentDto
{
    public string? Id { get; set;}
    public DateTime StartDateTimeUTC { get; set; }
    public DateTime EndDateTimeUTC { get; set; }
    public string? Description { get; set; }
    public string? ApplicationStateId { get; set; }
    public bool BrowserNotificationSent { get; set; }
    public bool EmailNotificationSent { get; set; }
}
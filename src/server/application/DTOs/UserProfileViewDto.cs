namespace SilverKinetics.w80.Application.DTOs;

public record UserProfileViewDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? TimeZone { get; set; }
    public string? Nickname { get; set; }
    public string? Culture { get; set; }
    public bool EnableAppointmentBrowserNotifications { get; set; }
    public bool EnableAppointmentEmailNotifications { get; set; }
}
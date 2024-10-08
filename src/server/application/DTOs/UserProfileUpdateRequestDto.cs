namespace SilverKinetics.w80.Application.DTOs;

public record UserProfileUpdateRequestDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? TimeZone { get; set; }
    public string? Nickname { get; set; }
    public string? Culture { get; set; }
    public string? Password { get; set; }
    public bool EnableAppointmentBrowserNotifications { get; set; }
    public bool EnableAppointmentEmailNotifications { get; set; }
    public DateTime? UpdatedUTC { get; set; }
    public DateTime? CreatedUTC { get; set; }
}
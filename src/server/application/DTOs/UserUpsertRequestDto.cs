using SilverKinetics.w80.Common;

namespace SilverKinetics.w80.Application.DTOs;

public record UserUpsertRequestDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? TimeZone { get; set; }
    public string? Nickname { get; set; }
    public string? Culture { get; set; }
    public Role Role { get; set; }
    public bool EnableEventBrowserNotifications { get; set; }
    public bool EnableEventEmailNotifications { get; set; }
}
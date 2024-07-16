using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Application.DTOs;

public record ApplicationUpdateRequestDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string CompanyName { get; set; }
    public decimal? CompensationMin { get; set; }
    public decimal? CompensationMax { get; set; }
    public CompensationType? CompensationType { get; set; }
    public string? Role { get; set; }
    public string RoleDescription { get; set; }
    public PositionType? PositionType { get; set; }
    public WorkSetting? WorkSetting { get; set; }
    public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();
    public string? SourceOfJobPosting  { get; set; }
    public List<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
    public string? Industry { get; set; }
    public string? HQLocation { get; set; }
    public string? PositionLocation { get; set; }
    public string? TravelRequirements { get; set; }
    public string? AdditionalInfo { get; set; }
    public List<StateDto> States { get; set; } = new List<StateDto>();
}
using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Application.DTOs;

public record ApplicationViewDto
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? CompanyName { get; set; }
    public decimal? CompensationMin { get; set; }
    public decimal? CompensationMax { get; set; }
    public CompensationType? CompensationType { get; set; }
    public string? Role { get; set; }
    public string? RoleDescription { get; set; }
    public PositionType? PositionType { get; set; }
    public WorkSetting? WorkSetting { get; set; }
    public List<ContactDto> Contacts { get; set; } = [];
    public string? SourceOfJobPosting  { get; set; }
    public List<AppointmentDto> Appointments { get; set; } = [];
    public string? Industry { get; set; }
    public string? HQLocation { get; set; }
    public string? PositionLocation { get; set; }
    public string? TravelRequirements { get; set; }
    public string? AdditionalInfo { get; set; }
    public List<StateDto> States { get; set; } = [];
    public RejectionDto Rejection { get; set; } = new RejectionDto();
    public AcceptanceDto Acceptance { get; set; } = new AcceptanceDto();

    public DateTime? UpdatedUTC { get; set; }
    public DateTime? ArchivedUTC { get; set; }
    public DateTime? DeactivatedUTC { get; set; }
    public DateTime? CreatedUTC { get; set; }
}
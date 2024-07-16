using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Application.DTOs;

public record AcceptanceDto
{
    public AcceptanceMethod Method { get; set; }
    public string? ResponseText { get; set; }
    public DateTime? AcceptedUTC { get; set; }
    public bool ArchiveOpenApplications { get; set; }
}
using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Application.DTOs;

public record RejectionDto
{
    public DateTime? RejectedUTC { get; set; }
    public RejectionMethod? Method { get; set; }
    public string? Reason { get; set;}
    public string? ResponseText { get; set; }
}
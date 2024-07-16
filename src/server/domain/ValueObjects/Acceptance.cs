using System.ComponentModel.DataAnnotations;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Acceptance
{
    public AcceptanceMethod Method { get; set; }
    [MaxLength(ResponseTextMaxSize)]
    public string? ReponseText { get; set; }
    public DateTime? AcceptedUTC { get; set; }

    public const int ResponseTextMaxSize = 2000;
}
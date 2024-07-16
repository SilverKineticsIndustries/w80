using System.ComponentModel.DataAnnotations;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Rejection
{
    public RejectionMethod Method { get; set; }
    [MaxLength(ReasonMaxSize)]
    public string Reason { get; set;}
    [MaxLength(ResponseTextMaxSize)]
    public string? ResponseText { get; set; }
    public DateTime? RejectedUTC { get; set; }

    public const int ReasonMaxSize = 200;
    public const int ResponseTextMaxSize = 2000;
}
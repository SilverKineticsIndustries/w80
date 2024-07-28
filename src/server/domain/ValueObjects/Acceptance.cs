using System.ComponentModel.DataAnnotations;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Acceptance
{
    public AcceptanceMethod Method { get; private set; }
    [MaxLength(ResponseTextMaxSize)]
    public string? ReponseText { get; set; }
    public DateTime AcceptedUTC { get; private set; }

    private Acceptance() {}
    public Acceptance(AcceptanceMethod method)
    {
        Method = method;
        AcceptedUTC = DateTime.UtcNow;
    }

    public static Acceptance Empty = new Acceptance();

    public const int ResponseTextMaxSize = 2000;
}
using System.ComponentModel.DataAnnotations;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Rejection
{
    public RejectionMethod Method { get; private set; }
    [MaxLength(ReasonMaxSize)]
    public string Reason { get; private set; }
    [MaxLength(ResponseTextMaxSize)]
    public string? ResponseText { get; set; }
    public DateTime RejectedUTC { get; private set; }

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Rejection() {}
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Rejection(RejectionMethod method, string reason)
    {
        Method = method;
        Reason = reason;
        RejectedUTC = DateTime.UtcNow;
    }

    public static Rejection Empty = new Rejection();

    public const int ReasonMaxSize = 200;
    public const int ResponseTextMaxSize = 2000;
}
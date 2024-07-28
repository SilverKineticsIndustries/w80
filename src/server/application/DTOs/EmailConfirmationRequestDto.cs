namespace SilverKinetics.w80.Application.DTOs;

public record EmailConfirmationRequestDto
{
    public string? Code { get; set; }
}
namespace SilverKinetics.w80.Application.DTOs;

public record EmailConfirmationResponseDto
{
    public bool Success { get; set; }
    public string[]? InfoMessages { get; set; }
}
namespace SilverKinetics.w80.Application.DTOs;

public record EmailConfirmationGenerationResponseDto
{
    public bool Success { get; set; }
    public string? InfoMessage { get; set ;}
}
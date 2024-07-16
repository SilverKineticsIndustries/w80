namespace SilverKinetics.w80.Application.DTOs;

public record EmailConfirmationGenerationRequestDto
{
    public string Email { get; set; }
}
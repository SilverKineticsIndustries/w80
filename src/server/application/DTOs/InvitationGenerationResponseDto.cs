namespace SilverKinetics.w80.Application.DTOs;

public record InvitationGenerationResponseDto
{
    public bool Success { get; set; }
    public string Code { get; set; }
    public string InfoMessage { get; set ;}
}
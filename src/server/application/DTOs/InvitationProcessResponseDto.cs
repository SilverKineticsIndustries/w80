namespace SilverKinetics.w80.Application.DTOs;

public record InvitationProcessResponseDto
{
    public bool Success { get; set; }
    public string[]? InfoMessages { get; set; }
}
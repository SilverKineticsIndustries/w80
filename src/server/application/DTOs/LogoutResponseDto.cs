namespace SilverKinetics.w80.Application.DTOs;

public record LogoutResponseDto
{
    public bool Success { get; set; }
    public string? InfoMessage { get; set; }
}
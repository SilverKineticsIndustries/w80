namespace SilverKinetics.w80.Application.DTOs;

public record RefreshRequestDto
{
    public string RefreshToken { get; set; }
}
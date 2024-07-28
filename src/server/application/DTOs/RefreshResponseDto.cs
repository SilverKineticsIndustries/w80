namespace SilverKinetics.w80.Application.DTOs;

public record RefreshResponseDto
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpirationUTC { get; set; }
    public string? InfoMessage { get; set; }
}
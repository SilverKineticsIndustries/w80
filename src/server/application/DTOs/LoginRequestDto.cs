namespace SilverKinetics.w80.Application.DTOs;

public record LoginRequestDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Captcha { get; set; }
    public bool ResendEmailConfirmation { get; set; }
}
namespace SilverKinetics.w80.Application.DTOs;

public record InvitationProcessRequestDto
{
    public string Email { get; set; }
    public string InvitationCode { get; set; }
    public string Password { get; set; }
}
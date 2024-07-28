using SilverKinetics.w80.Common;

namespace SilverKinetics.w80.Application.DTOs;

public record UserViewDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public Role Role { get; set; }
    public string? TimeZone { get; set; }
    public string? Nickname { get; set; }
    public string? Culture { get; set; }
    public bool IsEmailOwnershipConfirmed { get; set; }
    public bool MustActivateWithInvitationCode { get; set; }
    public string? InvitationCode { get; set; }
    public string? CreatedBy { set; get; }
    public DateTime? CreatedUTC { set; get; }
    public string? UpdatedBy { set; get; }
    public DateTime? UpdatedUTC { get; set; }
    public string? DeactivatedBy { get; set; }
    public DateTime? DeactivatedUTC { get; set; }
}
using MongoDB.Bson;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

// Even though UserSecurity is not a domain ring concern, I am having problems
// putting this entity into the application ring because StorageContext
// is defined here (domain ring) and I need to add a line into
// StorageContext.OnModelCreating(...) to make it aware of this entity.

public sealed class UserSecurity
    : IVersionedEntity,
    IAggregateRoot
{
    public ObjectId Id { get; set; } // User.Id

    public string? PasswordHash { get; set; }
    public bool MustActivateWithInvitationCode { get; set; }
    public bool IsEmailOwnershipConfirmed { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpirationUTC { get; set; }

    public bool CanLogin()
    {
        return
            !string.IsNullOrWhiteSpace(PasswordHash);
    }

    public void InvalidateRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpirationUTC = null;
    }

    public void SetRefreshToken(string refreshTokenHash, DateTime expirationDate)
    {
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpirationUTC = expirationDate;
    }

    public string SetPassword(string clearTextPassword)
    {
        PasswordHash = Hash.HashPassword(clearTextPassword);
        return PasswordHash;
    }

    public static UserSecurity InitializeInactiveUser(
        User user)
    {
        var userSecurity = new UserSecurity();
        userSecurity.Id = user.Id;
        userSecurity.MustActivateWithInvitationCode = true;
        return userSecurity;
    }

    #region [ IVersionedEntity ]

    int IVersionedEntity.Version { get { return 1; } }

    #endregion
}


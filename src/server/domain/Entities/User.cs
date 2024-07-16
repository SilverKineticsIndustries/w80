using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class User
    : IVersionedEntity,
    IAggregateRoot,
    IHasUpdateAudit,
    IHasCreationAudit,
    ISoftDeletionEntity,
    IHasPreUpsertAction<User>
{
    public ObjectId Id { get; set; }

    [MaxLength(EmailMaxLength)]
    public string Email { get; set; }
    [MaxLength(NicknameMaxLength)]
    public string Nickname { get; set; }
    public string TimeZone { get; set; }
    public string Culture { get; set; }
    public Role Role { get; set; }
    public bool EnableEventBrowserNotifications { get; set; }
    public bool EnableEventEmailNotifications { get; set; }

    public ObjectId CreatedBy { set; get; }
    public DateTime CreatedUTC { set; get; }
    public ObjectId? UpdatedBy { set; get; }
    public DateTime? UpdatedUTC { get; set; }

    public ObjectId? DeactivatedBy { get; set; }
    public DateTime? DeactivatedUTC { get; set; }

    public bool IsDeactivated()
    {
        return DeactivatedUTC is not null && !DeactivatedUTC.Value.IsMaxOrMinValue();
    }

    public void Deactivate(IDateTimeProvider dateTimeProvider)
    {
        DeactivatedUTC = dateTimeProvider.GetUtcNow();
    }

    public void Deactivate(DateTime utcNow)
    {
        DeactivatedUTC = utcNow;
    }

    public bool IsServiceWorker()
    {
        return Role == Role.ServiceWorker;
    }

    public bool IsEmailMatch(string email)
    {
        if (Email == null || email == null)
            return false;

        return
            string.Equals(Email.Trim(), email.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    public string DisplayName()
    {
        return Nickname ?? Email;
    }

    public void CopyFrom(User? other)
    {
        if (other == null)
            return;

        Role = other.Role;
        UpdatedBy = other.UpdatedBy;
        UpdatedUTC = other.UpdatedUTC;
        CreatedBy = other.CreatedBy;
        CreatedUTC = other.CreatedUTC;
        DeactivatedBy = other.DeactivatedBy;
        DeactivatedUTC = other.DeactivatedUTC;
    }

    internal static User Create(string email, string? nickname = null)
    {
        var user = new User();
        user.Email = email;
        user.Nickname = nickname;
        user.TimeZone = Constants.DefaultTimeZone;
        user.Culture = SupportedCultures.DefaultCulture;
        return user;
    }

    public const int EmailMaxLength = 100;
    public const int NicknameMaxLength = 50;

    #region [ IVersionedEntity ]

    int IVersionedEntity.Version { get { return 1; } }

    #endregion
}

using MongoDB.Bson;
using Riok.Mapperly.Abstractions;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Application.DTOs;

namespace SilverKinetics.w80.Application.Mappers;

[Mapper(UseDeepCloning = true, RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public partial class UserMapper
{
    public static partial UserProfileViewDto ToDTO(User user);
    public static partial UserProfileUpdateRequestDto ToProfileUpdateDTO(User user);
    public static partial UserProfileUpdateRequestDto ToProfileUpdateDTO(UserProfileViewDto userProfile);

    public static User ToEntity(UserProfileUpdateRequestDto profile, Role role)
    {
        return new User(ObjectId.Parse(profile.Id), role, profile.Email ?? string.Empty)
        {
            Nickname = profile.Nickname,
            TimeZone = profile.TimeZone ?? SupportedTimeZones.DefaultTimezone,
            Culture = profile.Culture ?? SupportedCultures.DefaultCulture,
            EnableAppointmentBrowserNotifications = profile.EnableAppointmentBrowserNotifications,
            EnableAppointmentEmailNotifications = profile.EnableAppointmentEmailNotifications
        };
    }
    public static partial User ToEntity(UserUpsertRequestDto user);

    public static async Task<UserViewDto> ToDTOAsync(IUserRepository userRepository, User user, UserSecurity userSecurity)
    {
        var ids = new List<ObjectId>();
        ids.Add(user.CreatedBy);
        if (user.UpdatedBy.HasValue)
            ids.Add(user.UpdatedBy.Value);
        if (user.DeactivatedBy.HasValue)
            ids.Add(user.DeactivatedBy.Value);

        var userIdEmails = (await userRepository.GetManyAsync((x) => ids.Contains(x.Id), CancellationToken.None)).ToDictionary(x => x.Id, x => x.Email);

        var createdBy = userIdEmails.TryGetValue(user.CreatedBy, out string? value) ? value : user.CreatedBy.ObjectIdToStringId();
        var updatedBy = user.UpdatedBy.HasValue
                            ? (
                                userIdEmails.TryGetValue(user.UpdatedBy.Value, out string? updatedByEmail)
                                    ? updatedByEmail
                                    : user.UpdatedBy.Value.ObjectIdToStringId()
                              )
                            : string.Empty;
        var deactivatedBy = user.DeactivatedBy.HasValue
                            ? (
                                userIdEmails.TryGetValue(user.DeactivatedBy.Value, out string? deactivatedByEmail)
                                    ? deactivatedByEmail
                                    : user.DeactivatedBy.Value.ObjectIdToStringId()
                              )
                            : string.Empty;

        return new UserViewDto()
        {
            Id = user.Id.ObjectIdToStringId(),
            Email = user.Email,
            Nickname = user.Nickname,
            TimeZone = user.TimeZone,
            Culture = user.Culture,
            Role = user.Role,
            CreatedBy = createdBy,
            CreatedUTC = user.CreatedUTC,
            UpdatedBy = updatedBy,
            UpdatedUTC = user.UpdatedUTC,
            DeactivatedBy = deactivatedBy,
            DeactivatedUTC = user.DeactivatedUTC,
            IsEmailOwnershipConfirmed = userSecurity.IsEmailOwnershipConfirmed,
            MustActivateWithInvitationCode = userSecurity.MustActivateWithInvitationCode
        };
    }
}
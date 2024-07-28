using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Events.User;

namespace SilverKinetics.w80.Domain.Services.User;

public class UserUpsertService(
    ISecurityContext securityContext,
    ISystemEventSink systemEventSink,
    IDateTimeProvider dateTimeProvider,
    IUserRepository userRepo,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
      : IUserUpsertService
{
    public Entities.User Create(string email, string? nickname = null)
    {
        return new Entities.User(ObjectId.GenerateNewId(), Role.User, email) { Nickname = nickname };
    }

    public async Task UpsertAsync(Entities.User user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken)
    {
        await UpsertAsync([user], requestSourceInfo, cancellationToken);
    }

    public async Task UpsertAsync(Entities.User[] users, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken)
    {
        var ids = users.Select(x => x.Id);
        var current = (await userRepo
                                .GetManyAsync(x => ids.Contains(x.Id), cancellationToken)
                                .ConfigureAwait(false))
                                .Select(x => x.Id);

        var now = dateTimeProvider.GetUtcNow();
        foreach(var user in users)
        {
            if (!current.Contains(user.Id))
                systemEventSink.Add(new UserCreatedEvent(securityContext.UserId, now, user, requestSourceInfo));
            else
                systemEventSink.Add(new UserUpdatedEvent(securityContext.UserId, now, user, requestSourceInfo));
        }
    }

    public async Task<IValidationBag> ValidateProfileAsync(Entities.User user, CancellationToken cancellationToken)
    {
        return await InternalValidateAsync(false, user, cancellationToken);
    }

    public async Task<IValidationBag> ValidateFullyAsync(Entities.User user, CancellationToken cancellationToken)
    {
        return await InternalValidateAsync(true, user, cancellationToken);
    }

    protected async Task<IValidationBag> InternalValidateAsync(bool fullValidation, Entities.User user, CancellationToken cancellationToken)
    {
        var bag = new ValidationBag();
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            bag.Add(new ValidationItem(stringLocalizer["User email cannot be empty."]));
            goto end;
        }

        if (fullValidation)
        {
            if (user.Role == Role.None)
            {
                bag.Add(new ValidationItem(stringLocalizer["User role cannot be empty."]));
                goto end;
            }
        }

        if (user.Role == Role.ServiceWorker)
        {
            bag.Add(new ValidationItem(stringLocalizer["Invalid user."]));
            goto end;
        }

        var recWithSameEmail = await userRepo.FirstOrDefaultAsync(x => x.Id != user.Id && x.Email.ToLower().Trim() == user.Email.ToLower().Trim(), cancellationToken);
        if (recWithSameEmail is not null)
        {
            bag.Add(new ValidationItem(stringLocalizer["An account already exists with same email."]));
            goto end;
        }

        if (user.Email.Length > Entities.User.EmailMaxLength)
            bag.Add(new ValidationItem(stringLocalizer["User email max length is {0} characters.", Entities.User.EmailMaxLength]));

        if (!Regex.IsMatch(user.Email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            bag.Add(new ValidationItem(stringLocalizer["User email is in an invalid format."]));

        if (!string.IsNullOrWhiteSpace(user.Nickname) && user.Nickname.Length > Entities.User.NicknameMaxLength)
            bag.Add(new ValidationItem(stringLocalizer["User nickname max length is {0} characters.", Entities.User.NicknameMaxLength]));

        if (string.IsNullOrEmpty(user.Culture))
            bag.Add(new ValidationItem(stringLocalizer["A culture is mandatory."]));
        else
        {
            if (!SupportedCultures.Cultures.ContainsKey(user.Culture))
                bag.Add(new ValidationItem(stringLocalizer["Culture {0} is not a supported culture.", user.Culture]));
        }

        if (string.IsNullOrEmpty(user.TimeZone))
            bag.Add(new ValidationItem(stringLocalizer["A timezone is mandatory."]));
        else
        {
            if (!SupportedTimeZones.TimeZones.ContainsKey(user.TimeZone))
                bag.Add(new ValidationItem(stringLocalizer["Timezone {0} is not a supported timezone.", user.TimeZone]));
        }

        end:
            return bag;
    }
}

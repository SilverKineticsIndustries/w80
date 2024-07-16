using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Events.User;
using SilverKinetics.w80.Common.Security;

namespace SilverKinetics.w80.Domain.Services.User;

public class UserDeactivationService(
    ISecurityContext securityContext,
    ISystemEventSink systemEventSink,
    IDateTimeProvider dateTimeProvider,
    IUserRepository userRepo,
    IUserSecurityRepository userSecurityRepo,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer)
     : IUserDeactivationService
{
    public async Task<Entities.User> DeactivateAsync(Entities.User user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.GetUtcNow();
        systemEventSink.Add(new UserDeactivatedEvent(securityContext.UserId, now, user, requestSourceInfo));

        var userSecurity = await userSecurityRepo.GetSingleOrDefaultAsync(x => x.Id == user.Id, cancellationToken).ConfigureAwait(false);
        userSecurity.InvalidateRefreshToken();

        user.Deactivate(now);
        return user;
    }

    public async Task<IValidationBag> ValidateForDeactivationAsync(Entities.User user, CancellationToken cancellationToken)
    {
        var bag = new ValidationBag();
        if (user.IsServiceWorker())
        {
            bag.Add(new ValidationItem(stringLocalizer["User cannot be deactivated."]));
            goto end;
        }

        if (user.Id == securityContext.UserId)
        {
            bag.Add(new ValidationItem(stringLocalizer["User cannot deactivate their own account."]));
            goto end;
        }

        if (user.IsDeactivated())
        {
            bag.Add(new ValidationItem(stringLocalizer["User cannot be deactivated because they are already in a deactivated state."]));
            goto end;
        }

        if (user.Role == Role.Administrator)
        {
            if (!await userRepo.AnyAsync(x => x.Role == Role.Administrator && x.Id != user.Id, cancellationToken))
                bag.Add(new ValidationItem(stringLocalizer["Last administrator cannot be deactivated."]));
        }

        end:
            return bag;
    }
}

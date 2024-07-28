using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Application.Security;

public static class Passwords
{
    public static IValidationBag Validate(IConfiguration config, IStringLocalizer stringLocalizer, string password, UserSecurity current)
    {
        var bag = new ValidationBag();

        if (string.IsNullOrWhiteSpace(password))
            bag.AddValidation(stringLocalizer["Password cannot be empty."]);

        if (password.Length < Convert.ToInt32(config[Keys.PasswordMinimumLength]))
            bag.AddValidation(stringLocalizer["Password must be at least {0} characters long.",
                                config.GetRequiredValue(Keys.PasswordMinimumLength)]);

        if (ArePasswordEqual(current, password))
            bag.AddValidation(stringLocalizer["New password is same as current password."]);

        return bag;
    }

    public static bool ArePasswordEqual(UserSecurity user, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return false;

        return Hash.IsPasswordEqual(newPassword, user.PasswordHash);
    }

    public static bool ArePasswordEqual(string currentPasswordHash, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPasswordHash))
            return false;

        return Hash.IsPasswordEqual(newPassword, currentPasswordHash);
    }
}
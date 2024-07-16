using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Domain.Services.User;

namespace SilverKinetics.w80.Domain.Services;

public class EmailMessageGenerator(
    IConfiguration config,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer
    ) : IEmailMessageGenerator
{
    public EmailNotificationMessage GetEmailAccountOwnershipVerificationEmailMessage(Entities.User user, string token)
    {
        var expirationDate = Convert.ToInt32(config[Keys.EmailConfirmationLifetimeInHours]);

        var domain = config[Keys.Domain];
        var encodedToken = UrlEncoder.Default.Encode(token);
        var confirmationUrl = $"https://{domain}/#?code={encodedToken}";

        var emailMessage = new EmailNotificationMessage();
        emailMessage.EmailAddresses = [user.Email];
        emailMessage.Template = TemplateType.EmailConfirmation;
        emailMessage.Culture = user.Culture;
        emailMessage.Subject = stringLocalizer["Email account ownership confirmation"];
        emailMessage.Parameters.Add("name", !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Email);
        emailMessage.Parameters.Add("confirmationUrl", confirmationUrl);
        emailMessage.Parameters.Add("confirmationExpirationLength", expirationDate.ToString());

        return emailMessage;
    }

    public EmailNotificationMessage GetEmailScheduleAlertMessage(Entities.User user, string companyName, int minutes)
    {
        var emailMessage = new EmailNotificationMessage();
        emailMessage.EmailAddresses = [user.Email];
        emailMessage.Template = TemplateType.EmailApplicationScheduleAlert;
        emailMessage.Culture = user.Culture;
        emailMessage.Subject = stringLocalizer["Appointment Alert"];
        emailMessage.Parameters.Add("name", !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Email);
        emailMessage.Parameters.Add("companyName", companyName);
        emailMessage.Parameters.Add("minutes", minutes.ToString());

        return emailMessage;
    }
}

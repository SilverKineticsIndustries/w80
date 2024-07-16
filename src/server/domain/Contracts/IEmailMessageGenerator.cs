using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Contracts;

public interface IEmailMessageGenerator
{
    EmailNotificationMessage GetEmailAccountOwnershipVerificationEmailMessage(
        User user, string verificationEmailToken);
    public EmailNotificationMessage GetEmailScheduleAlertMessage(
        User user, string companyName, int minutes);
}
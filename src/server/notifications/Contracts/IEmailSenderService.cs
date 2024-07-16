using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Notifications.Contracts;

public interface IEmailSenderService
{
    Task<bool> SendAsync(TemplateType Template, string subject, string body, string[] addresses);
}
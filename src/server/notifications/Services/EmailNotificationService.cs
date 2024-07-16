using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.ValueObjects;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Notifications.Services;

public class EmailNotificationService(IEmailSenderService emailSender, ITemplateResolver templateResolver)
    : IEmailNotificationService
{
    public async Task SendAsync(IEnumerable<EmailNotificationMessage> messages, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            messages,
            async (message, cancellationToken) =>
        {
            var body = await templateResolver.ResolveAsync(Templates.Meta[message.Template].FullyQualifiedName, message.Culture, message.Parameters);
            var subject = message.Subject;
            var to = message.EmailAddresses;
            await emailSender.SendAsync(message.Template, subject, body, to);

            cancellationToken.ThrowIfCancellationRequested();
        });
    }
}

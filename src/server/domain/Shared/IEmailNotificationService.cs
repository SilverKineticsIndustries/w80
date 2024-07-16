using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Shared;

public interface IEmailNotificationService
{
    Task SendAsync(IEnumerable<EmailNotificationMessage> messages, CancellationToken cancellationToken);
}


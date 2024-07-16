using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.TestHelper.Fakes;

public class EmailSenderServiceFake
    : IEmailSenderService
{
    public Lazy<List<(TemplateType Template, string Subject, string Body, string[] Addresses)>> Emails = new(() => []);

    public Task<bool> SendAsync(TemplateType template, string subject, string body, string[] addresses)
    {
        Emails.Value.Add((template, subject, body, addresses));
        return Task.FromResult(true);
    }
}
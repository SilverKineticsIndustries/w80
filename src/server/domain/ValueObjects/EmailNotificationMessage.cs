
namespace SilverKinetics.w80.Domain.ValueObjects;

public record EmailNotificationMessage
{
    public string Subject { get; private set; }
    public string Culture { get; private set ;}
    public string[] EmailAddresses { get; private set; }
    public TemplateType Template { get; private set; }
    public IDictionary<string,string> Parameters { get; set; } = new Dictionary<string,string>();

    public EmailNotificationMessage(string subject, string culture, string[] emailAddresses, TemplateType template)
    {
        Subject = subject;
        Culture = culture;
        EmailAddresses = emailAddresses;
        Template = template;
    }
}


namespace SilverKinetics.w80.Domain.ValueObjects;

public record EmailNotificationMessage
{
    public string Subject { get; set; }
    public string Culture { get; set ;}
    public string[] EmailAddresses { get; set; }
    public TemplateType Template { get; set; }
    public IDictionary<string,string> Parameters { get; set; } = new Dictionary<string,string>();
}

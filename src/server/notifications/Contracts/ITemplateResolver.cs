namespace SilverKinetics.w80.Notifications.Contracts;

public interface ITemplateResolver
{
    Task<string> ResolveAsync(string templateFullyQualifiedName, string culture, IDictionary<string,string>? parameters = null);
    string ReplaceParameters(string str, IDictionary<string,string> parameters);
}
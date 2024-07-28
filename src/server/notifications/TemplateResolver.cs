using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common;
using SilverKinetics.w80.Common.Configuration;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Notifications;

public class TemplateResolver(IConfiguration config)
    : ITemplateResolver
{
    public async Task<string> ResolveAsync(string templateFullyQualifiedName, string culture, IDictionary<string,string>? parameters = null)
    {
        var templatePath = $"{templateFullyQualifiedName}_{culture}.template";
        if (!_templateCache.ContainsKey(templatePath))
        {
            var resource = await Common.Resource.Resources.GetEmbeddedResourceFileAsync(_currentAssembly, templatePath);
            _templateCache.AddOrUpdate(templatePath, resource, (s,i) => resource);
        }
        var templateBody = _templateCache[templatePath];
        templateBody = ReplaceParameters(templateBody, parameters);
        return templateBody;
    }

    public string ReplaceParameters(string str, IDictionary<string,string>? parameters)
    {
        parameters ??= new Dictionary<string,string>();

        // TODO: Maybe too crude. Have to potentially think about XSS here ...
        foreach(var parameter in parameters.Union(GetGenericReplacementParameters()))
        {
            if (str.Contains(parameter.Key.WrapAsParameter()))
                str = str.Replace(parameter.Key.WrapAsParameter(), parameter.Value);
        }

        return str;
    }

    private IDictionary<string,string> GetGenericReplacementParameters()
    {
        var parameters = new Dictionary<string,string>();
        parameters.Add("domain", config.GetRequiredValue(Keys.Domain));
        parameters.Add("appname", config.GetOptionalValue(Keys.Appname, "W80"));
        return parameters;
    }

    private static readonly Assembly _currentAssembly = typeof(TemplateResolver).Assembly;
    private static readonly ConcurrentDictionary<string, string> _templateCache = new();
}
using System.Collections.ObjectModel;
using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Notifications;

public sealed class Templates
{
    public static IReadOnlyDictionary<TemplateType, (string FullyQualifiedName, IList<string> ReplaceableFields)> Meta { get { return _templateTypeCache; }}

    private static readonly IReadOnlyDictionary<TemplateType, (string FullyQualifiedName, IList<string> ReplaceableFields)> _templateTypeCache
        = new ReadOnlyDictionary<TemplateType, (string FullyQualifiedName, IList<string> ReplaceableFields)>(
            new Dictionary<TemplateType, (string FullyQualifiedName, IList<string> ReplaceableFields)>()
            {
                {
                    TemplateType.EmailConfirmation,
                    (
                        "SilverKinetics.w80.Notifications.Templates.Email.EmailConfirmation",
                        new List<string>() {
                            "name",
                            "confirmationUrl",
                            "confirmationExpirationLength"
                        }
                    )
                },
                {
                    TemplateType.EmailApplicationScheduleAlert,
                    (
                        "SilverKinetics.w80.Notifications.Templates.Email.EmailScheduleAlert",
                        new List<string>() {
                            "name",
                            "companyName",
                            "minutes"
                        }
                    )
                }
            }
        );
}
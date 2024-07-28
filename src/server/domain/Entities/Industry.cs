using MongoDB.Bson;
using SilverKinetics.w80.Domain.Contracts;

namespace SilverKinetics.w80.Domain.Entities;

public class Industry
    : IAggregateRoot,
    ISoftDeletionEntity,
    IHasTranslations
{
    public ObjectId Id { get; set; }

    public string Name { get; private set; }
    public int SeqNo { get; private set; }
    public IDictionary<string,string> Translations = new Dictionary<string,string>();

    public ObjectId? DeactivatedBy { get; set; }
    public DateTime? DeactivatedUTC { get; set; }

    public Industry(ObjectId id, string name, int seqNo)
    {
        Id = id;
        Name = name;
        SeqNo = seqNo;
    }

    public string GetString(string fieldName, string language)
    {
        if (fieldName != nameof(Name))
            throw new Exception($"Cannot perform translations because field {fieldName} does not exist on entity {nameof(Industry)}.");

        if (!string.IsNullOrEmpty(language)
                && Translations.ContainsKey(language)
                && !string.IsNullOrWhiteSpace(Translations[language]))
            return Translations[language];

        return Name;
    }
}
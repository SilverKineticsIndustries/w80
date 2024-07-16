using MongoDB.Bson;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record Contact
{
    public ObjectId Id { get; set; }
    public int SeqNo { get; set; }
    public ContactType Type { get; set;}
    public ContactRole Role { get; set; }
    public string ContactName { get; set; }
    public string ContactParameter { get; set; }
}
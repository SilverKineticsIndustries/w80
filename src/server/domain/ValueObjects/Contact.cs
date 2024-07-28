namespace SilverKinetics.w80.Domain.ValueObjects;

public record Contact
{
    public int SeqNo { get; private set; }
    public ContactType Type { get; private set;}
    public ContactRole Role { get; private set; }
    public string ContactName { get; private set; }
    public string ContactParameter { get; set; } = null!;

    public Contact(string contactName, int seqNo, ContactType type, ContactRole role)
    {
        SeqNo = seqNo;
        Type = type;
        Role = role;
        ContactName = contactName;
    }
}
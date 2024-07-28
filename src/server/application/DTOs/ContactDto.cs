using SilverKinetics.w80.Domain;

namespace SilverKinetics.w80.Application.DTOs;

public record ContactDto
{
    public int SeqNo { get; set;}
    public ContactType Type { get; set;}
    public ContactRole Role { get; set; }
    public string? ContactName { get; set; }
    public string? ContactParameter { get; set; }
}
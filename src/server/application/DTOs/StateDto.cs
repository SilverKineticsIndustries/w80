namespace SilverKinetics.w80.Application.DTOs;

public record StateDto
{
    public string? Id { get; set; }
    public bool IsCurrent { get; set;}
    public string? Name { get; set; }
    public string? Resource { get; set; }
    public int SeqNo { get; set; }
}
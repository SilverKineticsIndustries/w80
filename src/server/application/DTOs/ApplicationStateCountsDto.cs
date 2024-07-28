namespace SilverKinetics.w80.Application.DTOs;

public record ApplicationStateCountsDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
    public int Value { get; set; }
};
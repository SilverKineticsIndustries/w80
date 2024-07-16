namespace SilverKinetics.w80.Application.DTOs;

public record ApplicationStateCountsDto
{
    public string Name { get; set; }
    public string Color { get; set; }
    public int Value { get; set; }

    public ApplicationStateCountsDto(string name, string color, int value)
    {
        Name = name;
        Color = color;
        Value = value;
    }
};
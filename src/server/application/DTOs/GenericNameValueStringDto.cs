namespace SilverKinetics.w80.Application.DTOs;

public record GenericNameValueStringDto
{
    public string Name { get; set; }
    public string Value { get; set; }

    public GenericNameValueStringDto() {}
    public GenericNameValueStringDto(string name, string value)
    {
        Name = name;
        Value = value;
    }
};
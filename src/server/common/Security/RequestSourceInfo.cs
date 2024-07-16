namespace SilverKinetics.w80.Common.Security;

public record RequestSourceInfo
{
    public string IP { get; set; }
    public string Host { get; set; }
    public IDictionary<string,string> Headers { get; set; } = new Dictionary<string,string>();

    public static readonly RequestSourceInfo Empty = new();
}
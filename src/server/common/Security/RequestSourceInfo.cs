namespace SilverKinetics.w80.Common.Security;

public record RequestSourceInfo
{
    public required string IP { get; set; }
    public required string Host { get; set; }
    public IDictionary<string,string> Headers { get; set; } = new Dictionary<string,string>();

    public static readonly RequestSourceInfo Empty = new RequestSourceInfo() {
        IP = "127.0.0.1",
        Host = "localhost"
    };
}
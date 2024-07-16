namespace SilverKinetics.w80.Common.Contracts;

public interface IValidationItem
{
    public string Message { get; }
    public bool IsError { get; }
    public IReadOnlyDictionary<string, string> Related { get; }
}
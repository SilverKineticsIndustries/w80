using System.Collections.ObjectModel;
using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Common.Validation;

public record ValidationItem(
    string msg,
    bool isError = true,
    IDictionary<string, string>? related = null)
        : IValidationItem
{
    public string Message { get; } = msg;

    public bool IsError { get; } = isError;

    public IReadOnlyDictionary<string, string> Related { get; }
        = new ReadOnlyDictionary<string, string>(related is not null
            ? related
            : new Dictionary<string, string>());
}
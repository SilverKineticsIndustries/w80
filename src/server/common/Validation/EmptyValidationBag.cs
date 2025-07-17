using System.Collections;
using System.Collections.Concurrent;
using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Common.Validation;

public class EmptyValidationBag : IValidationBag
{
    public bool IsEmpty { get { return true; } }
    public bool HasErrors { get { return false; } }

    public IValidationBag Add(IValidationItem item)
    {
        return this;
    }

    public void Merge(IValidationBag bag) {}

    public IEnumerator<IValidationItem> GetEnumerator()
    {
        return _concurrentBag.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _concurrentBag.GetEnumerator();
    }

    public static Lazy<EmptyValidationBag> Instance = new();

    private readonly ConcurrentBag<IValidationItem> _concurrentBag = [];
}
using System.Collections;
using System.Collections.Concurrent;
using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Common.Validation;

public class ValidationBag : IValidationBag
{
    public bool IsEmpty { get { return _concurrentBag.IsEmpty; } } 
    public bool HasErrors { get { return _concurrentBag.Any(x => x.IsError); } } 

    public IValidationBag Add(IValidationItem item)
    {
        _concurrentBag.Add(item);
        return this;
    }

    public void Merge(IValidationBag bag)
    {
        foreach(var item in bag)
            _concurrentBag.Add(item);
    }

    public IEnumerator<IValidationItem> GetEnumerator()
    {
        return _concurrentBag.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _concurrentBag.GetEnumerator();
    }

    private readonly ConcurrentBag<IValidationItem> _concurrentBag = [];
}
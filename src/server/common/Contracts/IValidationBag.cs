namespace SilverKinetics.w80.Common.Contracts;

public interface IValidationBag : IEnumerable<IValidationItem>
{
    bool IsEmpty { get; }
    bool HasErrors { get; }
    IValidationBag Add(IValidationItem item);
    void Merge(IValidationBag bag);
}
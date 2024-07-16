namespace SilverKinetics.w80.Common.Contracts;

public interface IDateTimeProvider 
{
    DateTime GetUtcNow();
}
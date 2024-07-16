using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Common;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }
}

namespace SilverKinetics.w80.Domain.Contracts;

public interface IHasTranslations
{
    string GetString(string fieldName, string culture);
}
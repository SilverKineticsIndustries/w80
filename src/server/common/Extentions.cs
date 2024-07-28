using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;

namespace SilverKinetics.w80.Common;

public static class Extensions
{
    public const string WrapBeginToken = "{{";
    public const string WrapEndToken = "}}";

    public static string WrapAsParameter(this string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            throw new ArgumentNullException(nameof(parameter), "Parameter name cannot be empty/blank/null.");

        if (parameter.Contains(WrapBeginToken) || parameter.Contains(WrapEndToken))
            throw new ArgumentException("Parameter name has already been wrapped.", nameof(parameter));

        return WrapBeginToken + parameter + WrapEndToken;
    }

    public static string UnwrapAsParameter(this string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            throw new ArgumentNullException(nameof(parameter), "Parameter name cannot be empty/blank/null.");

        if (!parameter.Contains(WrapBeginToken) && !parameter.Contains(WrapEndToken))
            throw new ArgumentException("Parameter name has already been unwrapped.", nameof(parameter));

        if (parameter.StartsWith(WrapBeginToken))
            parameter = parameter[2..];
        if (parameter.EndsWith(WrapEndToken))
            parameter = parameter[..^2];

        return parameter;
    }

    public static IValidationBag AddValidation(this IValidationBag bag, string clientMessage)
    {
        return
            bag.Add(new ValidationItem(clientMessage));
    }

    public static string ToISO8610String(this DateTime dateTime)
    {
        return dateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static DateTime FromISO8610String(this string dateTime)
    {
        return DateTimeOffset.Parse(dateTime, System.Globalization.CultureInfo.InvariantCulture).UtcDateTime;
    }

    public static bool IsMinValue(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.MinValue.Date;
    }

    public static bool IsMaxValue(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.MaxValue.Date;
    }

    public static bool IsMaxOrMinValue(this DateTime dateTime)
    {
        return IsMinValue(dateTime) || IsMaxValue(dateTime);
    }

    public static string GetRequiredValue(this IConfiguration config, string key)
    {
        var val = config[key];
        if (string.IsNullOrWhiteSpace(val))
            throw new Exception($"Required configuration {key} is missing or is empty.");

        return val;
    }

    public static string GetOptionalValue(this IConfiguration config, string key, string missingValue)
    {
        var val = config[key];
        if (string.IsNullOrWhiteSpace(val))
            return missingValue;

        return val;
    }
}
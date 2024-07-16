using MongoDB.Bson;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Common.Contracts;

namespace SilverKinetics.w80.Application.Mappers;

public static class Extensions
{
    public static IList<GenericNameValueStringDto> ToNameValueDtoList<T>(
        this IReadOnlyDictionary<string, T> dict,
        Func<T, string> valueToString)
    {
        return dict
            .Select(x =>
                new GenericNameValueStringDto()
                {
                    Name = valueToString(x.Value),
                    Value = x.Key
                }
            )
            .ToList();
    }

    public static IList<ValidationItemDto> ToValidationItemDtoList(this IValidationBag bag)
    {
        return
            bag.Select(x => new ValidationItemDto(x.Message)).ToList();
    }

    public static string ObjectIdToStringId(this ObjectId id) => id.ToString();
}
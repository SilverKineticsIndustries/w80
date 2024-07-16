namespace SilverKinetics.w80.Application.DTOs;

// Since we always have to validate on Update/Insert, and since we always return
// a "fresh" copy of the updated or inserted object, sometimes during the Update/Insert,
// the data might have errors which we need to return back to client. This generic DTO
// is used all over the place to encapsulate the returned object or the validation errors.
public record ComplexResponseDto<T>
{
    public T? Result { get; }
    public IList<ValidationItemDto> Errors { get; } = [];

    public ComplexResponseDto(T? result, IList<ValidationItemDto>? errors = null)
    {
        Result = result;
        if (errors != null)
            Errors = errors;
    }

    public ComplexResponseDto(IList<ValidationItemDto>? errors = null)
    {
        if (errors != null)
            Errors = errors;
    }
};
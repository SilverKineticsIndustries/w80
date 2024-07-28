using MongoDB.Bson;
using Riok.Mapperly.Abstractions;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Application.Mappers;

[Mapper(UseDeepCloning = true, RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public partial class ApplicationMapper
{
    public static partial ApplicationViewDto ToDTO(Domain.Entities.Application application);
    public static partial ApplicationUpdateRequestDto ToUpdateDTO(Domain.Entities.Application application);

    private static partial Domain.Entities.Application _ToEntity(ApplicationUpdateRequestDto application);

    [UserMapping(Default = true)]
    public static Domain.Entities.Application ToEntity(ApplicationUpdateRequestDto application)
    {
        var entity = _ToEntity(application);

        // Mapper is unable to map states because Application entity has ReadOnlyCollection<State>.
        // The only field which is changeable is IsCurrent, so we use our custom user mapping to
        // set this according to value from DTO.
        foreach(var state in application.States)
        {
            var match = entity.States.FirstOrDefault(x => x.Id == ObjectId.Parse(state.Id));
            if (match != null)
                match.IsCurrent = state.IsCurrent;
        }
        return entity;
    }

    public static partial Contact ToEntity(ContactDto contact);
    public static partial ContactDto ToDTO(Contact contact);

    public static partial State ToEntity(StateDto state);
    public static partial StateDto ToDTO(State state);

    public static partial RejectionDto ToDTO(Rejection rejection);
    public static partial Rejection ToEntity(RejectionDto rejection);

    public static partial AcceptanceDto ToDTO(Acceptance acceptance);
    public static partial Acceptance ToEntity(AcceptanceDto acceptance);
}
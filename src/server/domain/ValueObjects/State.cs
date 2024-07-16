using MongoDB.Bson;
using SilverKinetics.w80.Domain.Entities;

namespace SilverKinetics.w80.Domain.ValueObjects;

public record State
{
    public ObjectId Id { get; set; }
    public bool IsCurrent { get; set;}
    public string Name { get; set; }
    public string HexColor { get; set; }
    public int SeqNo { get; set; }

    public State() {}
    public State(ApplicationState applicationState)
    {
        Id = applicationState.Id;
        Name = applicationState.Name;
        HexColor = applicationState.HexColor;
        SeqNo = applicationState.SeqNo;
    }
}
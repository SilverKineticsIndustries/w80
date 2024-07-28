using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Domain.ValueObjects;

namespace SilverKinetics.w80.Domain.Entities;

public class Application
    : IVersionedEntity,
    IAggregateRoot,
    IHasUpdateAudit,
    IHasCreationAudit,
    ISoftDeletionEntity,
    IHasPreUpsertAction<Application>
{
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    [MaxLength(CompanyNameMaxLength)]
    public string CompanyName { get; set; } = null!;
    public string RoleDescription { get; set; } = null!;
    public decimal? CompensationMin { get; set; }
    public decimal? CompensationMax { get; set; }
    public CompensationType? CompensationType { get; set; }
    [MaxLength(RoleMaxLength)]
    public string? Role { get; set; }
    [MaxLength(RoleDescriptionMaxLength)]
    public PositionType? PositionType { get; set; }
    public WorkSetting? WorkSetting { get; set; }
    public List<Contact> Contacts { get; private set; }
    public string? SourceOfJobPosting { get; set; }
    public List<Appointment> Appointments { get; private set; }
    public string? Industry { get; set; }
    public string? HQLocation { get; set; }
    public string? PositionLocation { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? TravelRequirements { get; set; }
    public ReadOnlyCollection<State> States { get; private set; }
    public Rejection? Rejection { get; set; }
    public Acceptance? Acceptance { get; set; }

    public ObjectId CreatedBy { get; set;}
    public DateTime CreatedUTC { get; set; }
    public DateTime? UpdatedUTC { get; set; }
    public ObjectId? UpdatedBy { get; set; }
    public DateTime? ArchivedUTC { get; set; }
    public DateTime? DeactivatedUTC { get; set; }
    public ObjectId? DeactivatedBy { get; set; }

    public Application(ObjectId id, ObjectId userId, IEnumerable<ApplicationState> states)
    {
        Id = id;
        UserId = userId;
        Contacts = new List<Contact>();
        Appointments = new List<Appointment>();
        States = new ReadOnlyCollection<State>(Initialize(states));
    }

    public bool IsDeactivated()
    {
        return DeactivatedUTC is not null;
    }

    public bool IsArchived()
    {
        return ArchivedUTC is not null;
    }

    public bool IsRejected()
    {
        return Rejection?.RejectedUTC is not null;
    }

    public bool IsAccepted()
    {
        return Acceptance?.AcceptedUTC is not null;
    }

    // TODO: There are situations where states will be cleared and this will throw an error.
    public State GetCurrentState()
    {
        return States.Single(x => x.IsCurrent);
    }

    public void Deactivate(IDateTimeProvider dateTimeProvider)
    {
        DeactivatedUTC = dateTimeProvider.GetUtcNow();
        Appointments.Clear();
    }

    public void Reactivate()
    {
        DeactivatedUTC = null;
    }

    public void Archive(IDateTimeProvider dateTimeProvider)
    {
        ArchivedUTC = dateTimeProvider.GetUtcNow();
        Appointments.Clear();
    }

    public void Reject(Rejection rejection)
    {
        Rejection = rejection;
        Appointments.Clear();
    }

    public void Accept(Acceptance acceptance)
    {
        Acceptance = acceptance;
        Appointments.Clear();
    }

    public void Unarchive()
    {
        ArchivedUTC = null;
    }

    public void CopyFrom(Application? current)
    {
        if (current == null)
            return;

        CreatedBy = current.CreatedBy;
        CreatedUTC = current.CreatedUTC;
        UpdatedUTC = current.UpdatedUTC;
        UpdatedBy = current.UpdatedBy;
        ArchivedUTC = current.ArchivedUTC;
        DeactivatedBy = current.DeactivatedBy;
        DeactivatedUTC = current.DeactivatedUTC;

        if (current.Rejection != null)
            Rejection = current.Rejection with {};
        else
            Rejection = null;

        if (current.Acceptance != null)
            Acceptance = current.Acceptance with {};
        else
            Acceptance = null;
    }

    public IEnumerable<Appointment> GetScheduleEmailAlertsToSendOut(DateTime now, TimeSpan threshold)
    {
        if (IsDeactivated())
            return [];

        return Appointments.Where(x => !x.EmailNotificationSent && x.IsNowWithinThresholdOfEventStart(now, threshold));
    }

    private IList<State> Initialize(IEnumerable<ApplicationState> states)
    {
        var applicationStates = new List<State>();

        if (states == null || !states.Any())
            throw new ArgumentException("Parameter states must contain a list of all application states.", nameof(states));

        if (states.Count() != states.Select(x => x.SeqNo).Distinct().Count())
            throw new ArgumentException("Each application state must contain unique sequence number.", nameof(states));

        if (states.Where(x => !x.IsDeactivated()).Count() < 2)
            throw new ArgumentException("There must be at least two active application states.", nameof(states));

        foreach (var stat in states.Where(x => !x.IsDeactivated()))
            applicationStates.Add(new State(stat));

        applicationStates.OrderBy(status => status.SeqNo).First().IsCurrent = true;
        return applicationStates;
    }

    #region [ IVersionedEntity ]

    int IVersionedEntity.Version { get { return 1; } }

    #endregion

    public const int RoleMaxLength = 100;
    public const int CompanyNameMaxLength = 150;
    public const int RoleDescriptionMaxLength = 8000;

}
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
    public string CompanyName { get; set; }
    public decimal? CompensationMin { get; set; }
    public decimal? CompensationMax { get; set; }
    public CompensationType? CompensationType { get; set; }
    [MaxLength(RoleMaxLength)]
    public string? Role { get; set; }
    [MaxLength(RoleDescriptionMaxLength)]
    public string RoleDescription { get; set; }
    public PositionType? PositionType { get; set; }
    public WorkSetting? WorkSetting { get; set; }
    public List<Contact> Contacts { get; private set; }
    public string? SourceOfJobPosting { get; set; }
    public List<Appointment> Appointments { get; private set; }
    public string? Industry { get; set; }
    public string? HQLocation { get; set; }
    public string? PositionLocation { get; set; }
    public string? AdditionalInfo { get; set; }
    public string TravelRequirements { get; set; }
    public List<State> States { get; private set; }
    public Rejection Rejection { get; set; } = new Rejection();
    public Acceptance Acceptance { get; set; } = new Acceptance();

    public ObjectId CreatedBy { get; set;}
    public DateTime CreatedUTC { get; set; }
    public DateTime? UpdatedUTC { get; set; }
    public ObjectId? UpdatedBy { get; set; }
    public DateTime? ArchivedUTC { get; set; }
    public DateTime? DeactivatedUTC { get; set; }
    public ObjectId? DeactivatedBy { get; set; }

    public Application()
    {
        States = new List<State>();
        Contacts = new List<Contact>();
        Appointments = new List<Appointment>();
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

    public void Initialize(IEnumerable<ApplicationState> states)
    {
        if (states == null || !states.Any())
            throw new ArgumentException("Parameter states must contain a list of all application states.", nameof(states));

        if (states.Count() != states.Select(x => x.SeqNo).Distinct().Count())
            throw new ArgumentException("Each application state must contain unique sequence number.", nameof(states));

        if (states.Where(x => !x.IsDeactivated()).Count() < 2)
            throw new ArgumentException("There must be at least two active application states.", nameof(states));

        States.Clear();
        foreach (var stat in states.Where(x => !x.IsDeactivated()))
            States.Add(new State(stat));

        States.OrderBy(status => status.SeqNo).First().IsCurrent = true;
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

    public void Reject(IDateTimeProvider dateTimeProvider, Rejection rejection)
    {
        Rejection = rejection;
        Rejection.RejectedUTC = dateTimeProvider.GetUtcNow();
        Appointments.Clear();
    }

    public void Accept(IDateTimeProvider dateTimeProvider, Acceptance acceptance)
    {
        Acceptance = acceptance;
        Acceptance.AcceptedUTC = dateTimeProvider.GetUtcNow();
        Appointments.Clear();
    }

    public void Unarchive()
    {
        ArchivedUTC = null;
    }

    public void CopyFrom(Application? other)
    {
        if (other == null)
            return;

        CreatedBy = other.CreatedBy;
        CreatedUTC = other.CreatedUTC;
        UpdatedUTC = other.UpdatedUTC;
        UpdatedBy = other.UpdatedBy;
        ArchivedUTC = other.ArchivedUTC;
        DeactivatedBy = other.DeactivatedBy;
        DeactivatedUTC = other.DeactivatedUTC;

        Rejection = other.Rejection with {};
        Acceptance = other.Acceptance with {};
    }

    public IEnumerable<Appointment> GetScheduleEmailAlertsToSendOut(DateTime now, TimeSpan threshold)
    {
        if (IsDeactivated())
            return [];

        return Appointments.Where(x => !x.EmailNotificationSent && x.IsNowWithinThresholdOfEventStart(now, threshold));
    }

    #region [ IVersionedEntity ]

    int IVersionedEntity.Version { get { return 1; } }

    #endregion

    public const int RoleMaxLength = 100;
    public const int CompanyNameMaxLength = 150;
    public const int RoleDescriptionMaxLength = 8000;

}
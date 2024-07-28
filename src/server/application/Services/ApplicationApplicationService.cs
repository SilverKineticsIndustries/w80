using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.Mappers;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.Services;

public class ApplicationApplicationService(
    IMongoClient mongoClient,
    ISecurityContext securityContext,
    IApplicationUpsertService applicationUpsertService,
    IApplicationDeactivationService applicationDeactivationService,
    IApplicationRejectionService applicationRejectionService,
    IApplicationArchiveService applicationArchiveService,
    IApplicationAcceptanceService applicationAcceptanceService,
    IApplicationRepository applicationRepo)
    : IApplicationApplicationService
{
    public async Task<ApplicationViewDto> InitializeAsync(CancellationToken cancellationToken = default)
    {
        return
            ApplicationMapper.ToDTO(await applicationUpsertService.InitializeAsync(cancellationToken));
    }

    public async Task<ApplicationViewDto?> GetAsync(ObjectId id, CancellationToken cancellationToken= default)
    {
        var appl = await applicationRepo
                            .FirstOrDefaultAsync((x) => x.Id == id, cancellationToken)
                            .ConfigureAwait(false);

        if (appl is not null)
        {
            VerifyUserCanAccessApplication(appl.UserId);
            return ApplicationMapper.ToDTO(appl);
        }

        return null;
    }

    public async Task<IList<ApplicationViewDto>> GetForUser(ObjectId userId, bool includeDeactivated = false, CancellationToken cancellationToken = default)
    {
        if (!securityContext.CanAccess(userId))
            throw new AuthorizationException();

        var appls = await applicationRepo
                            .GetManyAsync(x => x.UserId == userId && (includeDeactivated || x.DeactivatedUTC == null), cancellationToken)
                            .ConfigureAwait(false);
        return
            new List<ApplicationViewDto>(appls.Select(x => ApplicationMapper.ToDTO(x)));
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> UpsertAsync(ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default)
    {
        var objId = ObjectId.Parse(application.Id);
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == objId, cancellationToken);
        VerifyUserCanAccessApplication(ObjectId.Parse(application.UserId), current);

        var entity = ApplicationMapper.ToEntity(application);
        entity.CopyFrom(current);

        var bag = await applicationUpsertService.ValidateAsync(entity, cancellationToken);
        if (bag.HasErrors)
            return
                new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

        await mongoClient.WrapInTransactionAsync(async
        (session) => {
            await applicationUpsertService.UpsertAsync(entity, cancellationToken);
            await applicationRepo.UpsertAsync(entity, cancellationToken, session);
        },
        cancellationToken);

        return
            new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(entity));
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> DeactivateAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var bag = await applicationDeactivationService.ValidateForDeactivationAsync(current, cancellationToken);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationDeactivationService.Deactivate(current);
                await applicationRepo.DeactivateAsync(current, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> ReactivateAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var bag = await applicationDeactivationService.ValidateForReactivationAsync(current, cancellationToken);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationDeactivationService.Reactivate(current);
                await applicationRepo.ReactivateAsync(current, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> ArchiveAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var bag = await applicationArchiveService.ValidateForArchiveAsync(current, cancellationToken);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationArchiveService.Archive(current);
                await applicationRepo.ArchiveAsync(current, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> RejectAsync(ObjectId id, RejectionDto rejection, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var reject = ApplicationMapper.ToEntity(rejection);
            var bag = applicationRejectionService.Validate(current, reject);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationRejectionService.Reject(current, reject);
                await applicationRepo.RejectAsync(current, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> AcceptAsync(ObjectId id, AcceptanceDto acceptance, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var accept = ApplicationMapper.ToEntity(acceptance);
            var bag = applicationAcceptanceService.Validate(current, accept);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationAcceptanceService.Accept(current, accept);
                var toArchive = new List<Domain.Entities.Application>();
                if (acceptance.ArchiveOpenApplications)
                    toArchive.AddRange(await applicationAcceptanceService.ArchiveAllOpenNotAcceptedApplications(current));

                await applicationRepo.AcceptAsync(current, toArchive, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<ApplicationViewDto>> UnarchiveAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            VerifyUserCanAccessApplication(current.UserId);

            var bag = await applicationArchiveService.ValidateForUnarchiveAsync(current);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<ApplicationViewDto>(bag.ToValidationItemDtoList());

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                applicationArchiveService.Unarchive(current);
                await applicationRepo.UnarchiveAsync(current, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<ApplicationViewDto>(ApplicationMapper.ToDTO(current));
        }
        return
            new ComplexResponseDto<ApplicationViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<IList<ValidationItemDto>> ValidateAsync(ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default)
    {
        var objId = ObjectId.Parse(application.Id);
        var current = await applicationRepo.FirstOrDefaultAsync(x => x.Id == objId, cancellationToken);
        VerifyUserCanAccessApplication(ObjectId.Parse(application.UserId), current);

        var entity = ApplicationMapper.ToEntity(application);
        entity.CopyFrom(current);

        var bag = await applicationUpsertService.ValidateAsync(entity, cancellationToken);
        return bag.ToValidationItemDtoList();
    }

    public async Task SetBrowserNotificationSentAsync(IDictionary<string, List<Guid>> evnts, CancellationToken cancellationToken)
    {
        var dict = evnts.ToDictionary(x => ObjectId.Parse(x.Key), x => x.Value);
        await applicationRepo.SetBrowserNotificationSentOnAppointmentsAsync(dict, cancellationToken);
    }

    protected void VerifyUserCanAccessApplication(ObjectId applicationUserId, Domain.Entities.Application? current = null)
    {
        // Here we are checking if the Application.UserId can be accessed by current user and
        // whether the Application.Id can be accessed by current user. The user might have changed that
        // Id (maliciously), but keep the UserId equal to theirs, in which case the system might
        // change someone else's record.

        if (!securityContext.CanAccess(applicationUserId) || (current != null && !securityContext.CanAccess(current.UserId)))
            throw new AuthorizationException();
    }
}
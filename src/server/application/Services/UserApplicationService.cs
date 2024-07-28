using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Configuration;
using SilverKinetics.w80.Domain.Shared;
using SilverKinetics.w80.Domain.Entities;
using SilverKinetics.w80.Common.Security;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Common.Contracts;
using SilverKinetics.w80.Common.Validation;
using SilverKinetics.w80.Application.Mappers;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Exceptions;

namespace SilverKinetics.w80.Application.Services;

public class UserApplicationService(
    IConfiguration config,
    IMongoClient mongoClient,
    ISecurityContext securityContext,
    IDateTimeProvider dateTimeProvider,
    IUserUpsertService userUpsertService,
    IUserDeactivationService userDeactivationService,
    IUserSecurityRepository userSecurityRepo,
    IStringLocalizer<Common.Resource.Resources> stringLocalizer,
    IUserRepository userRepo)
    : IUserApplicationService
{
    public async Task<IList<UserViewDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var output = new List<UserViewDto>();
        var users = await userRepo.GetManyAsync((x) => 1==1, cancellationToken).ConfigureAwait(false);
        var userSecurity = (await userSecurityRepo.GetManyAsync((x) => 1==1, cancellationToken).ConfigureAwait(false)).ToDictionary(x => x.Id, x => x);

        foreach(var user in users)
            output.Add(await UserMapper.ToDTOAsync(userRepo, user, userSecurity[user.Id]));

        return
            output;
    }

    public async Task<UserProfileViewDto?> GetProfileAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var user = await userRepo.FirstOrDefaultAsync((x) => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (user is not null)
        {
            VerifyUserCanAccessUser(user.Id);
            return UserMapper.ToDTO(user);
        }
        else
            return null;
    }

    public async Task<UserProfileViewDto?> GetProfileFromEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userRepo.FirstOrDefaultAsync((x) => x.Email == email, cancellationToken).ConfigureAwait(false);
        if (user is not null)
        {
            VerifyUserCanAccessUser(user.Id);
            return UserMapper.ToDTO(user);
        }
        else
            return null;
    }

    public async Task<ComplexResponseDto<UserProfileViewDto>> UpdateProfileAsync(UserProfileUpdateRequestDto userProfile, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken)
    {
        var objId = ObjectId.Parse(userProfile.Id);
        VerifyUserCanAccessUser(objId);

        var current = await userRepo.FirstOrDefaultAsync((x) => x.Id == objId, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            var bag = await ValidateProfileAsync(userProfile, current, cancellationToken);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<UserProfileViewDto>(bag.ToValidationItemDtoList());

            UserSecurity? userSecurity = null;
            if (!string.IsNullOrWhiteSpace(userProfile.Password))
            {
                userSecurity = await userSecurityRepo.FirstAsync(x => x.Id == objId, cancellationToken).ConfigureAwait(false);
                userSecurity.SetPassword(userProfile.Password);
            }

            var entity = UserMapper.ToEntity(userProfile, current.Role);

            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                await userUpsertService.UpsertAsync(entity, requestSourceInfo, cancellationToken);
                await userRepo.UpsertAsync(entity, current, cancellationToken, session);
                if (userSecurity != null)
                    await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken, session);
            },
            cancellationToken);

            return
                new ComplexResponseDto<UserProfileViewDto>(UserMapper.ToDTO(entity));
        }
        else
            return
                new ComplexResponseDto<UserProfileViewDto>([new ValidationItemDto("Item not found.")]);
    }

    public async Task<ComplexResponseDto<UserViewDto>> UpsertAsync(UserUpsertRequestDto user, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Id) || user.Id == ObjectId.Empty.ObjectIdToStringId())
            user.Id = ObjectId.GenerateNewId().ObjectIdToStringId();

        var objId = ObjectId.Parse(user.Id);
        VerifyUserCanAccessUser(objId);

        var current = await userRepo.FirstOrDefaultAsync((x) => x.Id == objId, cancellationToken).ConfigureAwait(false);
        bool isNew = current == null;

        var entity = UserMapper.ToEntity(user);
        var bag = await userUpsertService.ValidateFullyAsync(entity, cancellationToken);
        if (bag.HasErrors)
            return
                new ComplexResponseDto<UserViewDto>(bag.ToValidationItemDtoList());

        string? invitationCode = null;
        UserSecurity userSecurity = null!;

        await mongoClient.WrapInTransactionAsync(async
        (session) => {
            await userUpsertService.UpsertAsync(entity, requestSourceInfo, cancellationToken);
            await userRepo.UpsertAsync(entity, current, cancellationToken);

            if (isNew)
            {
                invitationCode = Convert.ToBase64String(Invitations.GenerateNew(config, entity.Email, dateTimeProvider.GetUtcNow()));
                userSecurity = UserSecurity.InitializeInactiveUser(entity);
                await userSecurityRepo.UpsertAsync(userSecurity, cancellationToken);
            } else
                userSecurity = await userSecurityRepo.FirstAsync((x) => x.Id == entity.Id, cancellationToken);
        },
        cancellationToken);

        var retDto = await UserMapper.ToDTOAsync(userRepo, entity, userSecurity);
        retDto.InvitationCode = invitationCode;

        return
            new ComplexResponseDto<UserViewDto>(retDto);
    }

    public async Task<ComplexResponseDto<UserViewDto>> DeactivateAsync(ObjectId id, RequestSourceInfo requestSourceInfo, CancellationToken cancellationToken = default)
    {
        VerifyUserCanAccessUser(id);

        var current = await userRepo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (current is not null)
        {
            var bag = await userDeactivationService.ValidateForDeactivationAsync(current, cancellationToken);
            if (bag.HasErrors)
                return
                    new ComplexResponseDto<UserViewDto>(bag.ToValidationItemDtoList());

            var userSecurity = await userSecurityRepo.FirstAsync(x => x.Id == id, cancellationToken);

            User user = null!;
            await mongoClient.WrapInTransactionAsync(async
            (session) => {
                user = await userDeactivationService.DeactivateAsync(current, requestSourceInfo, cancellationToken);
                await userRepo.DeactivateAsync(current, cancellationToken);
            },
            cancellationToken);

            return
                new ComplexResponseDto<UserViewDto>(await UserMapper.ToDTOAsync(userRepo, user, userSecurity));
        }
        return
            new ComplexResponseDto<UserViewDto>([new ValidationItemDto("Item not found.")]);
    }

    protected async Task<IValidationBag> ValidateProfileAsync(UserProfileUpdateRequestDto userProfile, User current, CancellationToken cancellationToken)
    {
        var bag = new ValidationBag();

        if (!string.IsNullOrEmpty(userProfile.Password))
        {
            var objId = ObjectId.Parse(userProfile.Id);
            var userSecurity = await userSecurityRepo.FirstAsync(x => x.Id == objId, cancellationToken).ConfigureAwait(false);
            bag.Merge(Passwords.Validate(config, stringLocalizer, userProfile.Password, userSecurity));
        }

        var entity = UserMapper.ToEntity(userProfile, current.Role);
        bag.Merge(await userUpsertService.ValidateProfileAsync(entity, cancellationToken));
        return bag;
    }

    protected void VerifyUserCanAccessUser(ObjectId userId)
    {
        if (!securityContext.CanAccess(userId))
            throw new AuthorizationException();
    }
}
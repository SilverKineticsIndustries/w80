using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Application;

namespace SilverKinetics.w80.Controller.Controllers;

[ApiController]
public class UserController(
    ISecurityContext securityContext,
    IUserApplicationService userApplicationService)
    : ControllerBase
{
    [HttpGet("/user/all")]
    [Authorize(Policy = Policies.AdministratorOnly)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        return this.OkOrNotFound(await userApplicationService.GetAllAsync(cancellationToken));
    }

    [HttpPost("/user")]
    [Authorize(Policy = Policies.AdministratorOnly)]
    public async Task<IActionResult> UpsertAsync(UserUpsertRequestDto userUpsertRequestDto, CancellationToken cancellationToken)
    {
        return this.OkOrValidationErrors(await userApplicationService.UpsertAsync(userUpsertRequestDto, HttpContext.GetRequestSourceInfo(), cancellationToken));
    }

    [HttpGet("/user/profile")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> GetAsync([FromQuery] string? userId, CancellationToken cancellationToken)
    {
        var targetUserId = string.IsNullOrEmpty(userId) ? securityContext.UserId : ObjectId.Parse(userId);
        if (!this.CanCurrentUserPerformActionOnTargetUser(targetUserId))
            return Unauthorized();

        return this.OkOrNotFound(await userApplicationService.GetProfileAsync(targetUserId, cancellationToken));
    }

    [HttpPost("/user/profile")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> UpdateProfileAsync([FromBody]UserProfileUpdateRequestDto userProfile, CancellationToken cancellationToken)
    {
        if (!this.CanCurrentUserPerformActionOnTargetUser(ObjectId.Parse(userProfile.Id)))
            return Unauthorized();

        return this.OkOrValidationErrors(await userApplicationService.UpdateProfileAsync(userProfile, HttpContext.GetRequestSourceInfo(), cancellationToken));
    }

    [HttpPost("/user/deactivate"), Produces("application/json")]
    [Authorize(Policy = Policies.AdministratorOnly)]
    public async Task<IActionResult> DeactivateAsync(string userId, CancellationToken cancellationToken)
    {
        var objectId = ObjectId.Parse(userId);
        if (!this.CanCurrentUserPerformActionOnTargetUser(objectId))
            return Unauthorized();

        return this.OkOrValidationErrors(await userApplicationService.DeactivateAsync(objectId, HttpContext.GetRequestSourceInfo(), cancellationToken));
    }
}

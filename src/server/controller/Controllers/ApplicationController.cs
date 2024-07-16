using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Application.Security;
using MongoDB.Bson;

namespace SilverKinetics.w80.Controller.Controllers;

[ApiController]
public class ApplicationController(
    IApplicationApplicationService applicationApplicationService)
    : ControllerBase
{
    [HttpGet("/application/create")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> CreateAsync(CancellationToken cancellationToken = default)
    {
        return
            this.OkOrNotFound(await applicationApplicationService.InitializeAsync(cancellationToken));
    }

    [HttpGet("/application")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> GetAsync([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrNotFound(await applicationApplicationService.GetAsync(ObjectId.Parse(id), cancellationToken));
    }

    [HttpPost("/application/deactivate")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> DeactivateAsync([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.DeactivateAsync(ObjectId.Parse(id), cancellationToken));
    }

    [HttpPost("/application/reactivate")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> ReactivateAsync([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.ReactivateAsync(ObjectId.Parse(id), cancellationToken));
    }

    [HttpPost("/application/archive")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> ArchiveAsync([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.ArchiveAsync(ObjectId.Parse(id), cancellationToken));
    }

    [HttpPost("/application/unarchive")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> UnarachiveAsync([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.UnarchiveAsync(ObjectId.Parse(id), cancellationToken));
    }

    [HttpGet("/application/foruser/")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> GetForUserAsync([FromQuery] string userId, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrNotFound(await applicationApplicationService.GetForUser(ObjectId.Parse(userId), includeDeactivated: false, cancellationToken));
    }

    [HttpGet("/application/validate")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> ValidateAsync([FromBody] ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrNotFound(await applicationApplicationService.ValidateAsync(application, cancellationToken));
    }

    [HttpPost("/application/reject")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> RejectAsync([FromQuery] string id, RejectionDto rejected, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.RejectAsync(ObjectId.Parse(id), rejected ?? new RejectionDto(), cancellationToken));
    }

    [HttpPost("/application/accept")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> AcceptAsync([FromQuery] string id, AcceptanceDto acceptance, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.AcceptAsync(ObjectId.Parse(id), acceptance ?? new AcceptanceDto(), cancellationToken));
    }

    [HttpPost("/application")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> UpsertAsync([FromBody]ApplicationUpdateRequestDto application, CancellationToken cancellationToken = default)
    {
        return
            this.OkOrValidationErrors(await applicationApplicationService.UpsertAsync(application, cancellationToken));
    }

    [HttpPost("/application/appointments/marksent")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> SetBrowserNotificationSendAsync([FromBody]Dictionary<string, List<Guid>> evnts, CancellationToken cancellationToken = default)
    {
        await applicationApplicationService.SetBrowserNotificationSentAsync(evnts, cancellationToken);
        return
            Ok(evnts);
    }
}

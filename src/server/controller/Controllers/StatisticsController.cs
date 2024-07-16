using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SilverKinetics.w80.Domain.Contracts;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Application.Contracts;

namespace SilverKinetics.w80.Controller.Controllers;

[ApiController]
public class StatisticsController(
    ISecurityContext securityContext,
    IStatisticsApplicationService statisticsApplicationService)
    : ControllerBase
{
    [HttpGet("/statistics")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> GetStatiticsAsync(CancellationToken cancellationToken = default)
    {
        //var targetUserId = userId == default ? securityContext.UserId : userId;
        var targetUserId = securityContext.UserId;
        if (!this.CanCurrentUserPerformActionOnTargetUser(targetUserId))
            return Unauthorized();

        var data = await statisticsApplicationService.GetAsync(targetUserId, cancellationToken);
        return this.Ok(data);
    }
}

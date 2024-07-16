using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SilverKinetics.w80.Application.Contracts;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Notifications.Contracts;

namespace SilverKinetics.w80.Controller.Controllers;

[ApiController]
public class OptionsController(IOptionsApplicationService optionsApplicationService, IEmailSenderService emailSender)
    : ControllerBase
{
    [HttpGet("/options/cultures")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public IActionResult GetSupportedCultures()
    {
        return Ok(optionsApplicationService.GetSupportedCultures());
    }

    [HttpGet("/options/timezones")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public IActionResult GetSupportedTimezones()
    {
        return Ok(optionsApplicationService.GetSupportedTimezones());
    }

    [HttpGet("/options/industries")]
    [Authorize(Policy = Policies.UserOrAdministrator)]
    public async Task<IActionResult> GetIndustriesAsync(CancellationToken cancellationToken)
    {
        return Ok(await optionsApplicationService.GetIndustriesAsync(cancellationToken));
    }
}

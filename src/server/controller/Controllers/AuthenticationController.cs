using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SilverKinetics.w80.Application;
using SilverKinetics.w80.Application.DTOs;
using SilverKinetics.w80.Application.Security;
using SilverKinetics.w80.Application.Contracts;

namespace SilverKinetics.w80.Controller.Controllers;

[ApiController]
public class AuthenticationController
    : ControllerBase
{
    public AuthenticationController(
        IConfiguration configuration,
        IAuthenticationApplicationService authenticationApplicationService,
        IReCaptchaApplicationService reCaptchaApplicationService)
    {
        _cookieManager = new CookieManager(this, configuration);
        _reCaptchaApplicationService = reCaptchaApplicationService;
        _authenticationApplicationService = authenticationApplicationService;
    }

    [Route("/authentication/login")]
    [HttpPost, Produces("application/json")]
    public async Task<IActionResult> LoginWithCredentialsAsync(LoginRequestDto loginRequest, CancellationToken cancellationToken)
    {
        await _reCaptchaApplicationService.VerifyRequestNotFromBotAsync(loginRequest.Captcha ?? string.Empty, cancellationToken);

        var response = await _authenticationApplicationService.LoginWithCredentialsAsync(loginRequest, _cookieManager, HttpContext.GetRequestSourceInfo(), cancellationToken);
        if (response.Success)
        {
            return Ok(new {
                response.AccessToken,
                response.RefreshTokenExpirationUTC
            });
        }
        else if (response.EmailConfirmationSent)
        {
            return Ok(new {
                EmailVerificationSent = true
            });
        }
        else
            return Unauthorized(new ProblemDetails() {
                Title = response.InfoMessage
            });
    }

    [Route(Tokens.RefreshTokenEndPointPath)]
    [HttpPost, Produces("application/json")]
    public async Task<IActionResult> LoginWithRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var request = new RefreshRequestDto {
            RefreshToken = _cookieManager.GetCookie(Tokens.RefreshTokenCookieName)
        };

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unauthorized(new ProblemDetails() {
                Type = ProblemDetailsTypes.InvalidRefreshToken.ToString()
            });

        var response = await _authenticationApplicationService.LoginWithRefreshTokenAsync(request, _cookieManager, HttpContext.GetRequestSourceInfo(), cancellationToken);
        if (response.Success)
        {
            return Ok(new {
                response.AccessToken,
                response.RefreshTokenExpirationUTC
            });
        }
        else
            return Unauthorized(new ProblemDetails() {
                Type = ProblemDetailsTypes.InvalidRefreshToken.ToString(),
                Title = response.InfoMessage
            });
    }

    [Route(Tokens.LogoutTokenEndPointPath)]
    [HttpPost, Produces("application/json")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var response = await _authenticationApplicationService.LogoutAsync(_cookieManager, HttpContext.GetRequestSourceInfo(), cancellationToken);
        if (response.Success)
            return Ok();
        else
            return BadRequest(new ProblemDetails() {
                Title = response.InfoMessage,
            });
    }

    [Route("/authentication/invitation")]
    [HttpPost, Produces("application/json")]
    public async Task<IActionResult> ProcessInvitationAsync(InvitationProcessRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authenticationApplicationService.ProcessInvitationAsync(request, cancellationToken);
        if (response.Success)
            return Ok();
        else
            return BadRequest(string.Join(Environment.NewLine, response.InfoMessages ?? []));
    }

    [Route("/authentication/invitation/generate")]
    [HttpGet, Produces("application/json")]
    [Authorize(Policy = Policies.AdministratorOnly)]
    public IActionResult GenerateInvitationCode([FromQuery] InvitationGenerationRequestDto request)
    {
        return Ok(_authenticationApplicationService.GenerateInvitationCode(request));
    }

    [Route("/authentication/emailConfirmation")]
    [HttpPost, Produces("application/json")]
    public async Task<IActionResult> ProcessEmailConfirmationTokenAsync(EmailConfirmationRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authenticationApplicationService.ProcessEmailConfirmationTokenAsync(request, HttpContext.GetRequestSourceInfo(), cancellationToken);
        if (response.Success)
            return Ok();
        else
            return BadRequest(response.InfoMessages != null ? string.Join(Environment.NewLine, response.InfoMessages) : string.Empty);
    }

    [Route("/authentication/emailConfirmation/resend")]
    [HttpGet, Produces("application/json")]
    [Authorize(Policy = Policies.AdministratorOnly)]
    public async Task<IActionResult> ResendEmailConfirmationAsync([FromQuery] EmailConfirmationGenerationRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _authenticationApplicationService.ResendEmailConfirmationAsync(request, cancellationToken));
    }

    private readonly ICookieManager _cookieManager;
    private readonly IReCaptchaApplicationService _reCaptchaApplicationService;
    private readonly IAuthenticationApplicationService _authenticationApplicationService;
}